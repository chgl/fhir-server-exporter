using AwesomeAssertions;
using FakeItEasy;
using FhirServerExporter;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

public class FhirExporterTests
{
    [Fact]
    public void Construct_WithCorrectConfig_ShouldSucceed()
    {
        var appConfig = new AppConfig { FhirServerUrl = new Uri("http://localhost:8082/fhir") };

        var sut = new FhirExporter(
            A.Fake<ILogger<FhirExporter>>(),
            Options.Create(appConfig),
            A.Fake<IFhirResourceCounter>()
        );

        sut.Should().NotBeNull();
    }
}
