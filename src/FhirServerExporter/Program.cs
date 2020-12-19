using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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
        .ConfigureServices((_, services) => services.AddHostedService<FhirExporter>());

CreateHostBuilder(args).Build().Run();

public class FhirExporter : BackgroundService
{
    private static readonly Gauge ResourceCount = Metrics
        .CreateGauge("fhir_resource_count", "Number of resources stored within the FHIR server by type.",
            new GaugeConfiguration
            {
                LabelNames = new[] { "type" }
            });

    private static readonly Counter FetchResourceCountErrors = Metrics
        .CreateCounter("fhir_fetch_resource_count_failed_total", "Number of resource count fetch operations that failed.",
            new CounterConfiguration
            {
                LabelNames = new[] { "type" }
            });

    private static readonly Histogram FetchResourceCountDuration = Metrics
        .CreateHistogram("fhir_fetch_resource_count_duration_seconds", "Histogram of resource count fetching durations.");

    private readonly ILogger<FhirExporter> _logger;
    private readonly IConfiguration _config;
    private readonly FhirClient _fhirClient;
    private readonly IEnumerable<string> _resourceTypes;

    public FhirExporter(ILogger<FhirExporter> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;

        var serverUrl = _config.GetValue<string>("FhirServerUrl") ??
            throw new InvalidOperationException("FhirServerUrl needs to be set.");

        var basicAuthUser = _config.GetValue<string>("Auth:Basic:Username", null);
        var basicAuthPassword = _config.GetValue<string>("Auth:Basic:Password", null);

        if (!string.IsNullOrWhiteSpace(basicAuthUser) && !string.IsNullOrWhiteSpace(basicAuthPassword))
        {
            _fhirClient = new FhirClient(serverUrl,
                messageHandler: GetBasicAuthMessageHandler(basicAuthUser, basicAuthPassword));
        }
        else
        {
            _fhirClient = new FhirClient(serverUrl);
        }

        var excludedResources = _config.GetValue("ExcludedResources", "").Split(',');

        _logger.LogInformation("Excluding the following resources from counting: {excludedResources}",
            _config.GetValue("ExcludedResources", ""));

        _resourceTypes = Enum.GetValues(typeof(Fhir.ResourceType))
                .Cast<Fhir.ResourceType>()
                .Except(new[] { Fhir.ResourceType.DomainResource, Fhir.ResourceType.Resource })
                .Select(s => s.ToString())
                .Except(excludedResources);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var port = _config.GetValue<int>("MetricsPort");
        var fetchInterval = TimeSpan.FromSeconds(_config.GetValue<int>("FetchIntervalSeconds"));

        var server = new MetricServer(port: port);
        server.Start();

        _logger.LogInformation("FHIR Server Prometheus Exporter running on port {port} for {fhirServerUrl}",
            port, _fhirClient.Endpoint);

        while (!stoppingToken.IsCancellationRequested)
        {
            using (FetchResourceCountDuration.NewTimer())
            {
                var updateTasks = _resourceTypes.Select(t => UpdateResourceCountAsync(t));
                await Task.WhenAll(updateTasks);
            }

            await Task.Delay(fetchInterval, stoppingToken);
        }

        await server.StopAsync();
    }

    private async Task UpdateResourceCountAsync(string resourceType)
    {
        _logger.LogDebug("Fetching resource count for {resourceType}", resourceType);
        try
        {
            var total = await FetchResourceCountForTypeAsync(resourceType);
            _logger.LogDebug("Updating resource count for {resourceType} to {count}",
                resourceType,
                total);

            ResourceCount.WithLabels(resourceType).Set(total);
        }
        catch (Exception exc)
        {
            _logger.LogError(exc, "Failed to fetch resource count for {resourceType}", resourceType);
            FetchResourceCountErrors.WithLabels(resourceType).Inc();
        }
    }

    private async Task<int> FetchResourceCountForTypeAsync(string resourceType)
    {
        var response = await _fhirClient.SearchAsync(resourceType, summary: SummaryType.Count);
        return response.Total.Value;
    }

    private static HttpMessageHandler GetBasicAuthMessageHandler(string username, string password) =>
        new HttpClientHandler
        {
            Credentials = new NetworkCredential(username, password),
        };
}
