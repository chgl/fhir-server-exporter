using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using Hl7.Fhir.Rest;
using Xunit;

namespace FhirServerExporter.Tests.E2E;

public class ContainerE2ETests : IAsyncLifetime
{
    private readonly TestcontainersContainer fhirServerContainer;
    private readonly IOutputConsumer fhirServerOutputConsumer;
    private readonly TestcontainersContainer fhirServerExporterContainer;
    private readonly IOutputConsumer fhirServerExporterOutputConsumer;
    private readonly IDockerNetwork containerNetwork;

    public ContainerE2ETests()
    {
        containerNetwork = new TestcontainersNetworkBuilder()
            .WithName("fhir-server-exporter-e2e")
            .Build();

        fhirServerOutputConsumer = Consume.RedirectStdoutAndStderrToStream(
            new MemoryStream(),
            new MemoryStream()
        );

        fhirServerContainer = new TestcontainersBuilder<TestcontainersContainer>()
            .WithImage("docker.io/hapiproject/hapi:v6.6.0")
            .WithName("fhir-server")
            .WithNetwork(containerNetwork)
            .WithExposedPort(8080)
            .WithPortBinding(8080, 8080)
            .WithCleanUp(true)
            .WithOutputConsumer(fhirServerOutputConsumer)
            .WithWaitStrategy(
                Wait.ForUnixContainer()
                    .UntilMessageIsLogged(
                        fhirServerOutputConsumer.Stdout,
                        ".*(Started Application).*"
                    )
            )
            .Build();

        var exporterImage =
            Environment.GetEnvironmentVariable("FHIR_SERVER_EXPORTER_E2E_TEST_IMAGE")
            ?? "ghcr.io/chgl/fhir-server-exporter:latest";

        fhirServerExporterOutputConsumer = Consume.RedirectStdoutAndStderrToStream(
            new MemoryStream(),
            new MemoryStream()
        );

        fhirServerExporterContainer = new TestcontainersBuilder<TestcontainersContainer>()
            .WithImage(exporterImage)
            .WithName("fhir-server-exporter")
            .WithNetwork(containerNetwork)
            .WithExposedPort(9797)
            .WithPortBinding(9797, 9797)
            .WithCleanUp(true)
            .WithEnvironment("FhirServerUrl", "http://fhir-server:8080/fhir")
            .WithOutputConsumer(fhirServerExporterOutputConsumer)
            .WithWaitStrategy(
                Wait.ForUnixContainer()
                    .UntilMessageIsLogged(
                        fhirServerExporterOutputConsumer.Stdout,
                        ".*(FHIR Server Prometheus Exporter running on port 9797).*"
                    )
            )
            .Build();
    }

    [Fact]
    public async Task GetFhirServerCapabilityStatement_ShouldSucceed()
    {
        var fhirClient = new Hl7.Fhir.Rest.FhirClient("http://localhost:8080/fhir");

        _ = await fhirClient.CapabilityStatementAsync();
    }

    [Fact]
    public async Task GetExporterMetricsEndpoint_ShouldSucceed()
    {
        var client = new HttpClient();
        var response = await client.GetAsync("http://localhost:9797/metrics");

        response.EnsureSuccessStatusCode();
    }

    public async Task InitializeAsync()
    {
        await this.containerNetwork.CreateAsync();
        await this.fhirServerContainer.StartAsync();
        await this.fhirServerExporterContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await this.fhirServerExporterContainer.DisposeAsync().AsTask();
        await this.fhirServerContainer.DisposeAsync().AsTask();
        this.fhirServerOutputConsumer.Dispose();
        this.fhirServerExporterOutputConsumer.Dispose();
        await this.containerNetwork.DeleteAsync();
    }
}
