using System.Text;
using System.Text.RegularExpressions;
using AwesomeAssertions;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using Xunit;

namespace FhirServerExporter.Tests.E2E;

public class LakehouseContainerTests : IAsyncLifetime
{
    private const string S3AccessKey = "admin";

#pragma warning disable RCS1181 // trailing comment on member declaration needed for gitleaks suppression
    private const string S3SecretKey = "seaweedpass"; // gitleaks:allow
#pragma warning restore RCS1181

    private const string S3BucketName = "fhir";

    /// <summary>Patient.ndjson contains exactly 10 patient resources.</summary>
    private const int ExpectedPatientCount = 10;

    private const int SeaweedFsS3Port = 8333;
    private const int SeaweedFsFilerPort = 8888;

    private static readonly Regex ResourceCountPatientRegex = new(
        @"fhir_resource_count\{[^}]*type=""Patient""[^}]*\}\s+(?<count>\d+(?:\.\d+)?)",
        RegexOptions.Multiline | RegexOptions.ExplicitCapture,
        TimeSpan.FromSeconds(1)
    );

    private readonly INetwork containerNetwork;
    private readonly IContainer seaweedFsContainer;
    private readonly IContainer pathlingContainer;
    private readonly IContainer fhirServerExporterContainer;

    public LakehouseContainerTests()
    {
        containerNetwork = new NetworkBuilder()
            .WithName($"fhir-lakehouse-e2e-{Guid.NewGuid()}")
            .Build();

        // "mini" runs the master, volume, filer, and S3 servers in a single process and
        // "-bucket" creates the bucket on startup, so no separate bucket setup step is needed.
        // Without an S3 config file, SeaweedFS uses the AWS_* environment variables as the
        // admin credentials. The filer serves the buckets as directories, so requesting the
        // bucket path there waits for the bucket to actually exist.
        seaweedFsContainer = new ContainerBuilder(
            "docker.io/chrislusf/seaweedfs:4.47@sha256:ce9e796f1fe6f06968f4c04bdaf8f678dad9c8acdfef3d244133d71bfa6bf882"
        )
            .WithName($"seaweedfs-{Guid.NewGuid()}")
            .WithNetwork(containerNetwork)
            .WithNetworkAliases("seaweedfs")
            .WithExposedPort(SeaweedFsS3Port)
            .WithPortBinding(SeaweedFsS3Port, assignRandomHostPort: true)
            .WithExposedPort(SeaweedFsFilerPort)
            .WithPortBinding(SeaweedFsFilerPort, assignRandomHostPort: true)
            .WithCommand("mini", "-dir=/data", $"-bucket={S3BucketName}")
            .WithEnvironment("AWS_ACCESS_KEY_ID", S3AccessKey)
            .WithEnvironment("AWS_SECRET_ACCESS_KEY", S3SecretKey)
            .WithCleanUp(cleanUp: true)
            .WithWaitStrategy(
                Wait.ForUnixContainer()
                    .UntilHttpRequestIsSucceeded(r =>
                        r.ForPort(SeaweedFsS3Port).ForPath("/healthz")
                    )
                    .UntilHttpRequestIsSucceeded(r =>
                        r.ForPort(SeaweedFsFilerPort).ForPath($"/buckets/{S3BucketName}/")
                    )
            )
            .Build();

        // Navigate from bin/Debug/net10.0 up to the repository root and then into hack/synthea-fhir-sample-data.
        // Using DirectoryInfo.Parent avoids the Path.Combine overload that silently drops earlier
        // arguments when a later segment is an absolute path.
        var assemblyDir = new DirectoryInfo(
            Path.GetDirectoryName(typeof(LakehouseContainerTests).Assembly.Location)!
        );
        var repoRoot =
            assemblyDir.Parent?.Parent?.Parent?.Parent?.Parent
            ?? throw new InvalidOperationException(
                "Could not determine the repository root from the assembly location."
            );
        var synthDataPath = Path.Join(repoRoot.FullName, "hack", "synthea-fhir-sample-data");

        pathlingContainer = new ContainerBuilder(
            "docker.io/aehrc/pathling:7.2.0@sha256:31b5ef50294e55136ae2278c2d0b8435a96a15b5da040ec785effb51875d08d3"
        )
            .WithName($"pathling-{Guid.NewGuid()}")
            .WithNetwork(containerNetwork)
            .WithNetworkAliases("pathling")
            .WithExposedPort(8080)
            .WithPortBinding(8080, true)
            .WithEnvironment("pathling.storage.warehouseUrl", $"s3a://{S3BucketName}")
            .WithEnvironment("pathling.storage.cacheDatasets", "false")
            .WithEnvironment("pathling.query.cacheResults", "false")
            .WithEnvironment("pathling.import.allowableSources", "file:///tmp/import/")
            .WithEnvironment("pathling.terminology.enabled", "false")
            .WithEnvironment("pathling.terminology.serverUrl", "http://localhost:8080/i-dont-exist")
            .WithEnvironment("fs.s3a.impl", "org.apache.hadoop.fs.s3a.S3AFileSystem")
            .WithEnvironment("fs.s3a.path.style.access", "true")
            .WithEnvironment("fs.s3a.endpoint", $"http://seaweedfs:{SeaweedFsS3Port}")
            .WithEnvironment("fs.s3a.access.key", S3AccessKey)
            .WithEnvironment("fs.s3a.secret.key", S3SecretKey)
            .WithEnvironment("spark.sql.parquet.compression.codec", "zstd")
            .WithEnvironment("spark.io.compression.codec", "zstd")
            .WithEnvironment("parquet.compression.codec.zstd.level", "9")
            .WithEnvironment("spark.serializer", "org.apache.spark.serializer.KryoSerializer")
            .WithEnvironment("spark.master", "local[*]")
            .WithEnvironment("spark.driver.memory", "4g")
            .WithEnvironment(
                "JAVA_TOOL_OPTIONS",
                "-Xmx4g -Xss64m -XX:G1HeapRegionSize=32M -XX:+ExplicitGCInvokesConcurrent -XX:+ExitOnOutOfMemoryError -Duser.timezone=UTC --add-exports=java.base/sun.nio.ch=ALL-UNNAMED --add-opens=java.base/java.net=ALL-UNNAMED --add-opens=java.base/java.nio=ALL-UNNAMED --add-opens=java.base/java.util=ALL-UNNAMED --add-opens=java.base/java.lang.invoke=ALL-UNNAMED"
            )
            .WithBindMount(synthDataPath, "/tmp/import")
            .WithCleanUp(cleanUp: true)
            .WithWaitStrategy(
                Wait.ForUnixContainer()
                    .UntilHttpRequestIsSucceeded(r => r.ForPort(8080).ForPath("/fhir/metadata"))
            )
            .Build();

        var exporterImage =
            Environment.GetEnvironmentVariable("FHIR_SERVER_EXPORTER_E2E_TEST_IMAGE")
            ?? "ghcr.io/chgl/fhir-server-exporter:latest";

        fhirServerExporterContainer = new ContainerBuilder(exporterImage)
            .WithName($"fhir-server-exporter-lakehouse-{Guid.NewGuid()}")
            .WithNetwork(containerNetwork)
            .WithExposedPort(9797)
            .WithPortBinding(9797, assignRandomHostPort: true)
            .WithEnvironment("FhirLakehouse__DatabasePath", $"s3://{S3BucketName}/default")
            .WithEnvironment("FhirLakehouse__S3__Endpoint", $"seaweedfs:{SeaweedFsS3Port}")
            .WithEnvironment("FhirLakehouse__S3__Region", "us-east-1")
            .WithEnvironment("FhirLakehouse__S3__UrlStyle", "path")
            .WithEnvironment("FhirLakehouse__S3__UseSsl", "false")
            .WithEnvironment("AWS_ACCESS_KEY_ID", S3AccessKey)
            .WithEnvironment("AWS_SECRET_ACCESS_KEY", S3SecretKey)
            .WithEnvironment("FetchIntervalSeconds", "5")
            .WithCleanUp(cleanUp: true)
            .WithWaitStrategy(
                Wait.ForUnixContainer()
                    .UntilHttpRequestIsSucceeded(r => r.ForPort(9797).ForPath("/metrics"))
            )
            .Build();
    }

    [Fact]
    public async Task GetExporterMetricsEndpoint_WithLakehouseCounter_ShouldSucceed()
    {
        var metricsUrl =
            $"http://localhost:{fhirServerExporterContainer.GetMappedPublicPort(9797)}/metrics";

        // Wait a bit for the exporter to perform the first fetch and update the metrics
        await Task.Delay(TimeSpan.FromSeconds(10));

        var body = await WaitForMetricsContainsAsync(
            metricsUrl,
            "fhir_resource_count",
            timeout: TimeSpan.FromSeconds(60)
        );

        // Matches a Prometheus line like: fhir_resource_count{type="Patient",server_name=""} 10
        var match = ResourceCountPatientRegex.Match(body);
        match
            .Success.Should()
            .BeTrue(
                $"the metrics response should contain a fhir_resource_count line for Patient {body}"
            );
        double.Parse(match.Groups["count"].Value, System.Globalization.CultureInfo.InvariantCulture)
            .Should()
            .Be(
                ExpectedPatientCount,
                "Patient.ndjson contains exactly {0} resources",
                ExpectedPatientCount
            );
    }

    public async Task InitializeAsync()
    {
        await containerNetwork.CreateAsync();

        await seaweedFsContainer.StartAsync();

        await pathlingContainer.StartAsync();

        // Import FHIR resources into Pathling to create Delta Lake tables in the SeaweedFS bucket
        const string ImportRequest = """
            {
                "resourceType": "Parameters",
                "parameter": [
                    {
                        "name": "source",
                        "part": [
                            { "name": "resourceType", "valueCode": "Patient" },
                            { "name": "url", "valueUrl": "file:///tmp/import/Patient.ndjson" },
                            { "name": "mode", "valueCode": "overwrite" }
                        ]
                    }
                ]
            }
            """;

        using var httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
        using var content = new StringContent(
            ImportRequest,
            Encoding.UTF8,
            "application/fhir+json"
        );
        var pathlingPort = pathlingContainer.GetMappedPublicPort(8080);
        var importResponse = await httpClient.PostAsync(
            $"http://localhost:{pathlingPort}/fhir/$import",
            content
        );
        importResponse.EnsureSuccessStatusCode();

        await fhirServerExporterContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await Task.WhenAll(
            fhirServerExporterContainer.DisposeAsync().AsTask(),
            pathlingContainer.DisposeAsync().AsTask(),
            seaweedFsContainer.DisposeAsync().AsTask()
        );
        await containerNetwork.DeleteAsync();
    }

    private static async Task<string> WaitForMetricsContainsAsync(
        string metricsUrl,
        string metricName,
        TimeSpan? timeout = null,
        TimeSpan? delay = null
    )
    {
        timeout ??= TimeSpan.FromSeconds(120);
        delay ??= TimeSpan.FromSeconds(5);

        using var client = new HttpClient();
        var deadline = DateTime.UtcNow.Add(timeout.Value);

        while (DateTime.UtcNow < deadline)
        {
            try
            {
                var response = await client.GetAsync(metricsUrl);

                if (response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();

                    if (body.Contains(metricName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return body;
                    }
                }
            }
            catch (HttpRequestException)
            {
                // Endpoint may not be ready yet, will retry
            }

            await Task.Delay(delay.Value);
        }

        throw new TimeoutException(
            $"Metric '{metricName}' not found in metrics response after {timeout.Value.TotalSeconds} seconds"
        );
    }
}
