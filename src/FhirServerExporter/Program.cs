using System.Web;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Microsoft.Extensions.Options;
using Prometheus;

static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration(
            (config) =>
            {
                config.AddYamlFile("queries.yaml", optional: true, reloadOnChange: true);
            }
        )
        .ConfigureServices(
            (ctx, services) =>
            {
                services.Configure<AuthConfig>(ctx.Configuration.GetSection("Auth"));
                services.Configure<AppConfig>(ctx.Configuration);

                services.AddSingleton<IAuthHeaderProvider, AuthHeaderProvider>();
                services.AddHostedService<FhirExporter>();

                services.AddAccessTokenManagement(
                    (isp, options) =>
                    {
                        var authConfig = isp.GetRequiredService<IOptions<AuthConfig>>().Value;

                        if (authConfig.OAuth.TokenUrl is not null)
                        {
                            options.Client.Clients.Add(
                                "default",
                                new()
                                {
                                    Address = authConfig.OAuth.TokenUrl.AbsoluteUri,
                                    ClientId = authConfig.OAuth.ClientId,
                                    ClientSecret = authConfig.OAuth.ClientSecret,
                                    Scope = authConfig.OAuth.Scope,
                                }
                            );
                        }
                    }
                );
            }
        )
        .ConfigureLogging(
            builder =>
                builder.AddSimpleConsole(options =>
                {
                    options.UseUtcTimestamp = true;
                    options.TimestampFormat = "yyyy-MM-ddTHH:mm:ssZ ";
                    options.SingleLine = false;
                })
        );

CreateHostBuilder(args).Build().Run();

public class FhirExporter : BackgroundService
{
    private static readonly Gauge ResourceCount = Metrics.CreateGauge(
        "fhir_resource_count",
        "Number of resources stored within the FHIR server by type.",
        new GaugeConfiguration { LabelNames = new[] { "type", "server_name" }, }
    );

    private static readonly Counter FetchResourceCountErrors = Metrics.CreateCounter(
        "fhir_fetch_resource_count_failed_total",
        "Number of resource count fetch operations that failed.",
        new CounterConfiguration { LabelNames = new[] { "type", "server_name" }, }
    );

    private static readonly Histogram FetchResourceCountDuration = Metrics.CreateHistogram(
        "fhir_fetch_resource_count_duration_seconds",
        "Histogram of resource count fetching durations.",
        new HistogramConfiguration { LabelNames = new[] { "server_name" }, }
    );

    private readonly ILogger<FhirExporter> log;
    private readonly AppConfig config;
    private readonly FhirClient fhirClient;
    private readonly IEnumerable<string> resourceTypes;
    private readonly IAuthHeaderProvider authHeaderProvider;
    private readonly string fhirServerName;
    private readonly IEnumerable<CustomMetric> customMetrics;
    private readonly IDictionary<string, Gauge> customGauges;

    public FhirExporter(
        ILogger<FhirExporter> logger,
        IOptions<AppConfig> config,
        IAuthHeaderProvider authHeaderProvider
    )
    {
        log = logger;
        this.config = config.Value;

        var serverUrl =
            this.config.FhirServerUrl
            ?? throw new InvalidOperationException(
                $"{nameof(this.config.FhirServerUrl)} needs to be set."
            );
        this.authHeaderProvider = authHeaderProvider;

        fhirClient = new FhirClient(serverUrl);

        var includedResources = ModelInfo.SupportedResources;

        if (!string.IsNullOrEmpty(this.config.IncludedResources))
        {
            log.LogInformation(
                "Including only the following resources for counting: {IncludedResources}",
                this.config.IncludedResources
            );
            includedResources = this.config.IncludedResources.Split(',').ToList();
        }

        var excludedResources = this.config.ExcludedResources.Split(',');

        log.LogInformation(
            "Excluding the following resources from counting: {ExcludedResources}",
            this.config.ExcludedResources
        );

        resourceTypes = includedResources.Except(excludedResources);

        fhirServerName = this.config.FhirServerName;

        customMetrics = this.config.Queries;

        customGauges = customMetrics
            .Where(metric => metric.Name is not null && metric.Query is not null)
            .Select(
                metric =>
                    Metrics.CreateGauge(
                        metric.Name ?? string.Empty,
                        metric.Description ?? string.Empty,
                        new GaugeConfiguration { LabelNames = new[] { "type", "server_name" }, }
                    )
            )
            .ToDictionary(gauge => gauge.Name);
    }

    protected override async System.Threading.Tasks.Task ExecuteAsync(
        CancellationToken stoppingToken
    )
    {
        var port = config.MetricsPort;
        var fetchInterval = TimeSpan.FromSeconds(config.FetchIntervalSeconds);

        using var server = new MetricServer(port: port);
        server.Start();

        log.LogInformation(
            "FHIR Server Prometheus Exporter running on port {port} for {fhirServerUrl}",
            port,
            fhirClient.Endpoint
        );

        while (!stoppingToken.IsCancellationRequested)
        {
            fhirClient.RequestHeaders!.Authorization = await authHeaderProvider.GetAuthHeaderAsync(
                stoppingToken
            );

            using (FetchResourceCountDuration.WithLabels(fhirServerName).NewTimer())
            {
                foreach (
                    var customMetric in customMetrics.Where(metrics => metrics.Query is not null)
                )
                {
                    log.LogDebug(
                        "Querying custom metric {name} using {query}",
                        customMetric.Name,
                        customMetric.Query
                    );

                    if (customMetric.Query is null || customMetric.Name is null)
                    {
                        log.LogWarning("Query or name for custom metric is unset");
                        continue;
                    }

                    var resourceTypeAndFilters = customMetric.Query.Split("?");

                    if (resourceTypeAndFilters.Length < 2)
                    {
                        log.LogWarning(
                            "Parsing custom metric query string failed. "
                                + "Should look like: <resourceType>?<name>=<value>&..."
                        );
                        continue;
                    }

                    var resourceType = resourceTypeAndFilters[0];
                    var kv = HttpUtility.ParseQueryString(resourceTypeAndFilters[1]);
                    var paramList = kv.AllKeys.Select(key => Tuple.Create(key, kv[key]));
                    var sp = SearchParams.FromUriParamList(paramList);
                    sp.Summary = SummaryType.Count;

                    var result = await fhirClient.SearchAsync(sp, resourceType);

                    if (result is null)
                    {
                        log.LogError(
                            "Response for search request using custom query {query} is null",
                            customMetric.Query
                        );
                        continue;
                    }

                    if (result.Total.HasValue)
                    {
                        customGauges[customMetric.Name]
                            .WithLabels(resourceType, fhirServerName)
                            .Set(result.Total.Value);
                    }
                    else
                    {
                        log.LogWarning("No 'total' returned for {query}", customMetric.Query);
                    }
                }

                foreach (var resourceType in resourceTypes)
                {
                    await UpdateResourceCountAsync(resourceType);
                }
            }

            await System.Threading.Tasks.Task.Delay(fetchInterval, stoppingToken);
        }

        await server.StopAsync();
    }

    private async System.Threading.Tasks.Task UpdateResourceCountAsync(string resourceType)
    {
        log.LogDebug("Fetching resource count for {resourceType}", resourceType);
        try
        {
            var total = await FetchResourceCountForTypeAsync(resourceType);
            log.LogDebug(
                "Updating resource count for {resourceType} to {count}",
                resourceType,
                total
            );

            if (total.HasValue)
            {
                ResourceCount.WithLabels(resourceType, fhirServerName).Set(total.Value);
            }
        }
        catch (Exception exc)
        {
            log.LogError(exc, "Failed to fetch resource count for {resourceType}", resourceType);
            FetchResourceCountErrors.WithLabels(resourceType, fhirServerName).Inc();
        }
    }

    private async Task<int?> FetchResourceCountForTypeAsync(string resourceType)
    {
        var response = await fhirClient.SearchAsync(resourceType, summary: SummaryType.Count);
        if (response is null)
        {
            log.LogError("Response for count request for {resourceType}", resourceType);
            return null;
        }

        return response.Total;
    }
}
