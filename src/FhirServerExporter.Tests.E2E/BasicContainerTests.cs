using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using FluentAssertions;
using Hl7.Fhir.Rest;
using Xunit;

namespace FhirServerExporter.Tests.E2E;

public class ContainerE2ETests : IAsyncLifetime
{
    private readonly IContainer fhirServerContainer;
    private readonly IOutputConsumer fhirServerOutputConsumer;
    private readonly IContainer fhirServerExporterContainer;
    private readonly INetwork containerNetwork;

    public ContainerE2ETests()
    {
        containerNetwork = new NetworkBuilder()
            .WithName($"fhir-server-exporter-e2e-{Guid.NewGuid()}")
            .Build();

        fhirServerOutputConsumer = Consume.RedirectStdoutAndStderrToStream(
            new MemoryStream(),
            new MemoryStream()
        );

        fhirServerContainer = new ContainerBuilder()
            .WithImage("docker.io/hapiproject/hapi:v6.10.1")
            .WithName("fhir-server")
            .WithNetwork(containerNetwork)
            .WithExposedPort(8080)
            .WithPortBinding(8080, 8080)
            .WithCleanUp(true)
            .WithOutputConsumer(fhirServerOutputConsumer)
            .WithWaitStrategy(
                Wait.ForUnixContainer()
                    .UntilHttpRequestIsSucceeded(r => r.ForPort(8080).ForPath("/fhir/metadata"))
            )
            .Build();

        var exporterImage =
            Environment.GetEnvironmentVariable("FHIR_SERVER_EXPORTER_E2E_TEST_IMAGE")
            ?? "ghcr.io/chgl/fhir-server-exporter:latest";

        fhirServerExporterContainer = new ContainerBuilder()
            .WithImage(exporterImage)
            .WithName("fhir-server-exporter")
            .WithNetwork(containerNetwork)
            .WithExposedPort(9797)
            .WithPortBinding(9797, 9797)
            .WithCleanUp(true)
            .WithEnvironment("FhirServerUrl", "http://fhir-server:8080/fhir")
            .WithWaitStrategy(
                Wait.ForUnixContainer()
                    .UntilHttpRequestIsSucceeded(r => r.ForPort(9797).ForPath("/metrics"))
            )
            .Build();
    }

    [Fact]
    public async Task GetFhirServerCapabilityStatement_ShouldSucceed()
    {
        var fhirClient = new FhirClient("http://localhost:8080/fhir");

        var statement = await fhirClient.CapabilityStatementAsync();

        statement.Should().NotBeNull();
    }

    [Fact]
    public async Task GetExporterMetricsEndpoint_ShouldSucceed()
    {
        var client = new HttpClient();
        var response = await client.GetAsync("http://localhost:9797/metrics");

        response.EnsureSuccessStatusCode();

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    public async Task InitializeAsync()
    {
        await containerNetwork.CreateAsync();
        await Task.WhenAll(
            fhirServerContainer.StartAsync(),
            fhirServerExporterContainer.StartAsync()
        );
    }

    public async Task DisposeAsync()
    {
        await this.fhirServerExporterContainer.DisposeAsync().AsTask();
        await this.fhirServerContainer.DisposeAsync().AsTask();
        this.fhirServerOutputConsumer.Dispose();
        await this.containerNetwork.DeleteAsync();
    }
}
