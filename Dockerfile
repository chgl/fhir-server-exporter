# kics false positive "Missing User Instruction": <https://docs.kics.io/latest/queries/dockerfile-queries/fd54f200-402c-4333-a5a4-36ef6709af2f/>
# kics-scan ignore-line
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/aspnet:9.0.3-noble-chiseled@sha256:dba2cfa6a751eac6ecc5371c2f7f08cad711edbf465afe8180c60be2853e7a5b AS runtime
WORKDIR /opt/fhir-server-exporter
EXPOSE 9797/tcp
USER 65532:65532
ENV ASPNETCORE_ENVIRONMENT="Production" \
    DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    ASPNETCORE_URLS="http://*:9797" \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0.201-noble@sha256:ee6f155e7735a5374808bee81a419ef60f057db3fbe4f7ad1aac7b7289514a85 AS build
WORKDIR "/build"
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1

COPY src/Directory.Build.props ./src/

COPY src/FhirServerExporter/FhirServerExporter.csproj ./src/FhirServerExporter/
COPY src/FhirServerExporter/packages.lock.json ./src/FhirServerExporter/

RUN dotnet restore --locked-mode /p:ContinuousIntegrationBuild=true src/FhirServerExporter/FhirServerExporter.csproj

COPY . .

RUN dotnet build src/FhirServerExporter/FhirServerExporter.csproj \
    --no-restore \
    --configuration Release

RUN dotnet publish src/FhirServerExporter/FhirServerExporter.csproj \
    --no-restore \
    --no-build \
    --configuration Release \
    -o /build/publish

FROM build AS test
WORKDIR /build

COPY src/Directory.Build.props ./src/

COPY src/FhirServerExporter.Tests/FhirServerExporter.Tests.csproj ./src/FhirServerExporter.Tests/

RUN dotnet restore --locked-mode src/FhirServerExporter.Tests/FhirServerExporter.Tests.csproj

WORKDIR /build/src/FhirServerExporter.Tests
RUN dotnet test \
    --configuration=Release \
    --collect:"XPlat Code Coverage" \
    --results-directory=./coverage \
    -l "console;verbosity=detailed"

FROM runtime
COPY --from=build /build/publish .
ENTRYPOINT ["dotnet", "FhirServerExporter.dll"]
