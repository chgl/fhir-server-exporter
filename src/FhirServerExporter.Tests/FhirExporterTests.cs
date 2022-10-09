using System;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

public class FhirExporterTests
{
    [Fact]
    public void Construct_WithMissingFhirServerUrl_ShouldThrow()
    {
        var appConfig = new AppConfig
        {
            FhirServerUrl = null,
        };

        Action construct = () => _ = new FhirExporter(A.Fake<ILogger<FhirExporter>>(), Options.Create(appConfig), A.Fake<IAuthHeaderProvider>());

        construct.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Construct_WithCorrectConfig_ShouldSucceed()
    {
        var appConfig = new AppConfig
        {
            FhirServerUrl = new Uri("http://localhost:8082/fhir"),
        };

        var sut = new FhirExporter(A.Fake<ILogger<FhirExporter>>(), Options.Create(appConfig), A.Fake<IAuthHeaderProvider>());

        sut.Should().NotBeNull();
    }
}
