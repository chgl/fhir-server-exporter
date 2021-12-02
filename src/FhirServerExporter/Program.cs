using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Hl7.Fhir.Rest;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prometheus;
using Fhir = Hl7.Fhir.Model;

static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((config) =>
        {
            config.AddYamlFile(
                "queries.yaml",
                optional: true,
                reloadOnChange: true);
        })
        .ConfigureServices((ctx, services) =>
        {
            services.AddSingleton<IAuthHeaderProvider, AuthHeaderProvider>();
            services.AddHostedService<FhirExporter>();

            var config = ctx.Configuration;
            services.AddAccessTokenManagement(options =>
            {
                var oauthUri = config.GetValue<Uri>("Auth:OAuth:TokenUrl")?.AbsoluteUri;
                if (oauthUri == null)
                {
                    return;
                }

                options.Client.Clients.Add("default", new()
                {
                    Address = oauthUri,
                    ClientId = config.GetValue<string>("Auth:OAuth:ClientId"),
                    ClientSecret = config.GetValue<string>("Auth:OAuth:ClientSecret"),
                    Scope = config.GetValue<string>("Auth:OAuth:Scope"),
                });
            });
        })
        .ConfigureLogging(builder =>
            builder.AddSimpleConsole(options =>
            {
                options.UseUtcTimestamp = true;
                options.TimestampFormat = "yyyy-MM-ddTHH:mm:ssZ ";
                options.SingleLine = false;
            }));

CreateHostBuilder(args).Build().Run();

public record CustomMetric
{
    public string Name { get; init; }

    public string Query { get; init; }

    public string Description { get; init; }
}

public class FhirExporter : BackgroundService
{
    private static readonly Gauge ResourceCount = Metrics
        .CreateGauge(
            "fhir_resource_count",
            "Number of resources stored within the FHIR server by type.",
            new GaugeConfiguration
            {
                LabelNames = new[] { "type", "server_name" },
            });

    private static readonly Counter FetchResourceCountErrors = Metrics
        .CreateCounter(
            "fhir_fetch_resource_count_failed_total",
            "Number of resource count fetch operations that failed.",
            new CounterConfiguration
            {
                LabelNames = new[] { "type", "server_name" },
            });

    private static readonly Histogram FetchResourceCountDuration = Metrics
        .CreateHistogram(
            "fhir_fetch_resource_count_duration_seconds",
            "Histogram of resource count fetching durations.",
            new HistogramConfiguration
            {
                LabelNames = new[] { "server_name" },
            });

    private readonly ILogger<FhirExporter> log;
    private readonly IConfiguration config;
    private readonly FhirClient fhirClient;
    private readonly IEnumerable<string> resourceTypes;
    private readonly IAuthHeaderProvider authHeaderProvider;
    private readonly string fhirServerName;
    private readonly IEnumerable<CustomMetric> customMetrics;
    private readonly IDictionary<string, Gauge> customGauges;

    public FhirExporter(ILogger<FhirExporter> logger, IConfiguration config, IAuthHeaderProvider authHeaderProvider)
    {
        log = logger;
        this.config = config;

        var serverUrl = this.config.GetValue<Uri>("FhirServerUrl");
        if (serverUrl == null)
        {
            throw new InvalidOperationException("FhirServerUrl needs to be set.");
        }

        this.authHeaderProvider = authHeaderProvider;

        fhirClient = new FhirClient(serverUrl);

        var excludedResources = this.config.GetValue("ExcludedResources", string.Empty).Split(',');

        log.LogInformation(
            "Excluding the following resources from counting: {excludedResources}",
            this.config.GetValue("ExcludedResources", string.Empty));

        resourceTypes = Enum.GetValues(typeof(Fhir.ResourceType))
                .Cast<Fhir.ResourceType>()
                .Except(new[] { Fhir.ResourceType.DomainResource, Fhir.ResourceType.Resource })
                .Select(s => s.ToString())
                .Except(excludedResources);

        fhirServerName = this.config.GetValue<string>("FhirServerName");

        customMetrics = config.GetSection("Queries").Get<List<CustomMetric>>() ?? new();

        customGauges = customMetrics.Select(metric => Metrics
            .CreateGauge(
                metric.Name,
                metric.Description,
                new GaugeConfiguration
                {
                    LabelNames = new[] { "type", "server_name" },
                }))
            .ToDictionary(gauge => gauge.Name);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var port = config.GetValue<int>("MetricsPort");
        var fetchInterval = TimeSpan.FromSeconds(config.GetValue<int>("FetchIntervalSeconds"));

        using var server = new MetricServer(port: port);
        server.Start();

        log.LogInformation(
            "FHIR Server Prometheus Exporter running on port {port} for {fhirServerUrl}",
            port,
            fhirClient.Endpoint);

        while (!stoppingToken.IsCancellationRequested)
        {
            fhirClient.RequestHeaders.Authorization = await authHeaderProvider.GetAuthHeaderAsync(stoppingToken);

            using (FetchResourceCountDuration.WithLabels(fhirServerName).NewTimer())
            {
                foreach (var customMetric in customMetrics)
                {
                    log.LogInformation("Querying custom metric {name} using {query}", customMetric.Name, customMetric.Query);
                    var resourceTypeAndFilters = customMetric.Query.Split("?");

                    if (resourceTypeAndFilters.Length < 2)
                    {
                        log.LogWarning("Parsing custom metric query string failed. " +
                            "Should look like: <resourceType>?<name>=<value>&...");
                        continue;
                    }

                    var resourceType = resourceTypeAndFilters[0];
                    var kv = HttpUtility.ParseQueryString(resourceTypeAndFilters[1]);
                    var paramList = kv.AllKeys.Select(key => Tuple.Create(key, kv[key]));
                    var sp = SearchParams.FromUriParamList(paramList);
                    sp.Summary = SummaryType.Count;

                    var result = await fhirClient.SearchAsync(sp, resourceType);

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

            await Task.Delay(fetchInterval, stoppingToken);
        }

        await server.StopAsync();
    }

    private async Task UpdateResourceCountAsync(string resourceType)
    {
        log.LogDebug("Fetching resource count for {resourceType}", resourceType);
        try
        {
            var total = await FetchResourceCountForTypeAsync(resourceType);
            log.LogDebug(
                "Updating resource count for {resourceType} to {count}",
                resourceType,
                total);

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
        return response.Total;
    }
}
