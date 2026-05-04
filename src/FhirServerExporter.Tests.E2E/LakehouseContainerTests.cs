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
    private const string MinioRootUser = "admin";

#pragma warning disable RCS1181 // trailing comment on member declaration needed for gitleaks suppression
    private const string MinioRootPassword = "miniopass"; // gitleaks:allow
#pragma warning restore RCS1181

    private const string MinioBucketName = "fhir";

    /// <summary>Patient.ndjson contains exactly 10 patient resources.</summary>
    private const int ExpectedPatientCount = 10;

    private static readonly Regex ResourceCountPatientRegex = new(
        @"fhir_resource_count\{[^}]*type=""Patient""[^}]*\}\s+(?<count>\d+(?:\.\d+)?)",
        RegexOptions.Multiline | RegexOptions.ExplicitCapture,
        TimeSpan.FromSeconds(1)
    );

    private readonly INetwork containerNetwork;
    private readonly IContainer minioContainer;
    private readonly IContainer pathlingContainer;
    private readonly IContainer fhirServerExporterContainer;

    public LakehouseContainerTests()
    {
        containerNetwork = new NetworkBuilder()
            .WithName($"fhir-lakehouse-e2e-{Guid.NewGuid()}")
            .Build();

        minioContainer = new ContainerBuilder(
            "docker.io/minio/minio:RELEASE.2025-09-07T16-13-09Z@sha256:14cea493d9a34af32f524e538b8346cf79f3321eff8e708c1e2960462bd8936e"
        )
            .WithName($"minio-{Guid.NewGuid()}")
            .WithNetwork(containerNetwork)
            .WithNetworkAliases("minio")
            .WithExposedPort(9000)
            .WithPortBinding(9000, true)
            .WithCommand("server", "/data", "--console-address", ":9001")
            .WithEnvironment("MINIO_UPDATE", "off")
            .WithEnvironment("MINIO_CALLHOME_ENABLE", "off")
            .WithEnvironment("MINIO_ROOT_USER", MinioRootUser)
            .WithEnvironment("MINIO_ROOT_PASSWORD", MinioRootPassword)
            .WithEnvironment("MINIO_BROWSER", "on")
            .WithEnvironment("MINIO_SCHEME", "http")
            .WithCleanUp(cleanUp: true)
            .WithWaitStrategy(
                Wait.ForUnixContainer()
                    .UntilHttpRequestIsSucceeded(r => r.ForPort(9000).ForPath("/minio/health/live"))
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
            .WithEnvironment("pathling.storage.warehouseUrl", $"s3a://{MinioBucketName}")
            .WithEnvironment("pathling.storage.cacheDatasets", "false")
            .WithEnvironment("pathling.query.cacheResults", "false")
            .WithEnvironment("pathling.import.allowableSources", "file:///tmp/import/")
            .WithEnvironment("pathling.terminology.enabled", "false")
            .WithEnvironment("pathling.terminology.serverUrl", "http://localhost:8080/i-dont-exist")
            .WithEnvironment("fs.s3a.impl", "org.apache.hadoop.fs.s3a.S3AFileSystem")
            .WithEnvironment("fs.s3a.path.style.access", "true")
            .WithEnvironment("fs.s3a.endpoint", "http://minio:9000")
            .WithEnvironment("fs.s3a.access.key", MinioRootUser)
            .WithEnvironment("fs.s3a.secret.key", MinioRootPassword)
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
            .WithEnvironment("FhirLakehouse__DatabasePath", $"s3://{MinioBucketName}/default")
            .WithEnvironment("FhirLakehouse__S3__Endpoint", "minio:9000")
            .WithEnvironment("FhirLakehouse__S3__Region", "us-east-1")
            .WithEnvironment("FhirLakehouse__S3__UrlStyle", "path")
            .WithEnvironment("FhirLakehouse__S3__UseSsl", "false")
            .WithEnvironment("AWS_ACCESS_KEY_ID", MinioRootUser)
            .WithEnvironment("AWS_SECRET_ACCESS_KEY", MinioRootPassword)
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

        await minioContainer.StartAsync();

        // Create the S3 bucket using the MinIO Client (mc) bundled in the MinIO image
        var createBucketResult = await minioContainer.ExecAsync([
            "/bin/bash",
            "-c",
            $"/usr/bin/mc alias set minio http://localhost:9000 {MinioRootUser} {MinioRootPassword} && /usr/bin/mc mb --ignore-existing minio/{MinioBucketName}",
        ]);
        createBucketResult.ExitCode.Should().Be(0);

        await pathlingContainer.StartAsync();

        // Import FHIR resources into Pathling to create Delta Lake tables in the MinIO bucket
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
            minioContainer.DisposeAsync().AsTask()
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

                    if (body.Contains(metricName))
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
