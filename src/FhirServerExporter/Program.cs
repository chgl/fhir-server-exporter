using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hl7.Fhir.Rest;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prometheus;
using Fhir = Hl7.Fhir.Model;

static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
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
                options.SingleLine = true;
            }));

CreateHostBuilder(args).Build().Run();

public class FhirExporter : BackgroundService
{
    private static readonly Gauge ResourceCount = Metrics
        .CreateGauge("fhir_resource_count", "Number of resources stored within the FHIR server by type.",
            new GaugeConfiguration
            {
                LabelNames = new[] { "type", "server_name" }
            });

    private static readonly Counter FetchResourceCountErrors = Metrics
        .CreateCounter("fhir_fetch_resource_count_failed_total", "Number of resource count fetch operations that failed.",
            new CounterConfiguration
            {
                LabelNames = new[] { "type", "server_name" }
            });

    private static readonly Histogram FetchResourceCountDuration = Metrics
        .CreateHistogram("fhir_fetch_resource_count_duration_seconds", "Histogram of resource count fetching durations.",
         new HistogramConfiguration
         {
             LabelNames = new[] { "server_name" }
         });

    private readonly ILogger<FhirExporter> _log;
    private readonly IConfiguration _config;
    private readonly FhirClient _fhirClient;
    private readonly IEnumerable<string> _resourceTypes;
    private readonly IAuthHeaderProvider _authHeaderProvider;
    private readonly string _fhirServerName;

    public FhirExporter(ILogger<FhirExporter> logger, IConfiguration config, IAuthHeaderProvider authHeaderProvider)
    {
        _log = logger;
        _config = config;

        var serverUrl = _config.GetValue<Uri>("FhirServerUrl");
        if (serverUrl == null)
        {
            throw new InvalidOperationException("FhirServerUrl needs to be set.");
        }

        _authHeaderProvider = authHeaderProvider;

        _fhirClient = new FhirClient(serverUrl);

        var excludedResources = _config.GetValue("ExcludedResources", "").Split(',');

        _log.LogInformation("Excluding the following resources from counting: {excludedResources}",
            _config.GetValue("ExcludedResources", ""));

        _resourceTypes = Enum.GetValues(typeof(Fhir.ResourceType))
                .Cast<Fhir.ResourceType>()
                .Except(new[] { Fhir.ResourceType.DomainResource, Fhir.ResourceType.Resource })
                .Select(s => s.ToString())
                .Except(excludedResources);

        _fhirServerName = _config.GetValue<string>("FhirServerName");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var port = _config.GetValue<int>("MetricsPort");
        var fetchInterval = TimeSpan.FromSeconds(_config.GetValue<int>("FetchIntervalSeconds"));

        using var server = new MetricServer(port: port);
        server.Start();

        _log.LogInformation("FHIR Server Prometheus Exporter running on port {port} for {fhirServerUrl}",
            port, _fhirClient.Endpoint);

        while (!stoppingToken.IsCancellationRequested)
        {
            _fhirClient.RequestHeaders.Authorization = await _authHeaderProvider.GetAuthHeaderAsync(stoppingToken);

            using (FetchResourceCountDuration.WithLabels(_fhirServerName).NewTimer())
            {
                foreach (var resourceType in _resourceTypes)
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
        _log.LogDebug("Fetching resource count for {resourceType}", resourceType);
        try
        {
            var total = await FetchResourceCountForTypeAsync(resourceType);
            _log.LogDebug("Updating resource count for {resourceType} to {count}",
                resourceType,
                total);

            if (total.HasValue)
            {
                ResourceCount.WithLabels(resourceType, _fhirServerName).Set(total.Value);
            }
        }
        catch (Exception exc)
        {
            _log.LogError(exc, "Failed to fetch resource count for {resourceType}", resourceType);
            FetchResourceCountErrors.WithLabels(resourceType, _fhirServerName).Inc();
        }
    }

    private async Task<int?> FetchResourceCountForTypeAsync(string resourceType)
    {
        var response = await _fhirClient.SearchAsync(resourceType, summary: SummaryType.Count);
        return response.Total;
    }
}
