using System.Web;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Microsoft.Extensions.Options;
using Prometheus;

namespace FhirServerExporter;

public class FhirExporter : BackgroundService
{
    private static readonly Gauge ResourceCount = Metrics.CreateGauge(
        "fhir_resource_count",
        "Number of resources stored within the FHIR server by type.",
        new GaugeConfiguration { LabelNames = ["type", "server_name"] }
    );

    private static readonly Counter FetchResourceCountErrors = Metrics.CreateCounter(
        "fhir_fetch_resource_count_failed_total",
        "Number of resource count fetch operations that failed.",
        new CounterConfiguration { LabelNames = ["type", "server_name"] }
    );

    private static readonly Histogram FetchResourceCountDuration = Metrics.CreateHistogram(
        "fhir_fetch_resource_count_duration_seconds",
        "Histogram of resource count fetching durations.",
        new HistogramConfiguration { LabelNames = ["server_name"] }
    );

    private readonly ILogger<FhirExporter> log;
    private readonly AppConfig config;
    private readonly IFhirResourceCounter resourceCounter;
    private readonly IEnumerable<ResourceType> resourceTypes;
    private readonly string fhirServerName;

    public FhirExporter(
        ILogger<FhirExporter> logger,
        IOptions<AppConfig> config,
        IFhirResourceCounter resourceCounter
    )
    {
        log = logger;
        this.config = config.Value;

        this.resourceCounter = resourceCounter;

        var includedResources = ModelInfo.SupportedResources;

        if (!string.IsNullOrEmpty(this.config.IncludedResources))
        {
            log.LogInformation(
                "Including only the following resources for counting: {IncludedResources}",
                this.config.IncludedResources
            );
            includedResources = [.. this.config.IncludedResources.Split(',')];
        }

        var excludedResources = this.config.ExcludedResources.Split(',');

        log.LogInformation(
            "Excluding the following resources from counting: {ExcludedResources}",
            this.config.ExcludedResources
        );

        resourceTypes = includedResources
            .Except(excludedResources, StringComparer.InvariantCultureIgnoreCase)
            .Select(r => Enum.Parse<ResourceType>(r));

        fhirServerName = this.config.FhirServerName;
    }

    protected override async System.Threading.Tasks.Task ExecuteAsync(
        CancellationToken stoppingToken
    )
    {
        var port = config.MetricsPort;
        var fetchInterval = TimeSpan.FromSeconds(config.FetchIntervalSeconds);

        using var server = new MetricServer(port: port);
        server.Start();

        log.LogInformation("FHIR Server Prometheus Exporter running on port {Port}", port);

        while (!stoppingToken.IsCancellationRequested)
        {
            using (FetchResourceCountDuration.WithLabels(fhirServerName).NewTimer())
            {
                foreach (var resourceType in resourceTypes)
                {
                    await UpdateResourceCountAsync(resourceType);
                }
            }

            await System.Threading.Tasks.Task.Delay(fetchInterval, stoppingToken);
        }

        await server.StopAsync();
    }

    private async System.Threading.Tasks.Task UpdateResourceCountAsync(ResourceType resourceType)
    {
        log.LogDebug("Fetching resource count for {ResourceType}", resourceType);
        try
        {
            var total = await FetchResourceCountForTypeAsync(resourceType);
            log.LogDebug(
                "Updating resource count for {ResourceType} to {Count}",
                resourceType,
                total
            );

            if (total.HasValue)
            {
                ResourceCount.WithLabels(resourceType.ToString(), fhirServerName).Set(total.Value);
            }
        }
        catch (Exception exc)
        {
            log.LogError(exc, "Failed to fetch resource count for {ResourceType}", resourceType);
            FetchResourceCountErrors.WithLabels(resourceType.ToString(), fhirServerName).Inc();
        }
    }

    private async Task<long?> FetchResourceCountForTypeAsync(ResourceType resourceType)
    {
        return await resourceCounter.GetResourceCountAsync(resourceType);
    }
}
