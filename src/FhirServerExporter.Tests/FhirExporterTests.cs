using System;
using System.Collections.Generic;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Xunit;

public class FhirExporterTests
{
    [Fact]
    public void Construct_WithMissingFhirServerUrl_ShouldThrow()
    {
        Action construct = () => _ = new FhirExporter(A.Fake<ILogger<FhirExporter>>(), A.Fake<IConfiguration>(), A.Fake<IAuthHeaderProvider>());

        construct.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Construct_WithCorrectConfig_ShouldSucceed()
    {
        var inMemorySettings = new Dictionary<string, string>
        {
            {"FhirServerUrl", "http://localhost/fhir"},
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var sut = new FhirExporter(A.Fake<ILogger<FhirExporter>>(), configuration, A.Fake<IAuthHeaderProvider>());

        sut.Should().NotBeNull();
    }
}
