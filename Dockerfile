# kics false positive "Missing User Instruction": <https://docs.kics.io/latest/queries/dockerfile-queries/fd54f200-402c-4333-a5a4-36ef6709af2f/>
# kics-scan ignore-line
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/aspnet:8.0.7-noble-chiseled@sha256:1f6d3352e053938ab1496d30a03b0f2fb6d45a7359aa0979ace5eca71cb3fec4 AS runtime
WORKDIR /opt/fhir-server-exporter
EXPOSE 9797/tcp
USER 65532:65532
ENV ASPNETCORE_ENVIRONMENT="Production" \
    DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    ASPNETCORE_URLS="http://*:9797" \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0.303-noble@sha256:487fcee12549e1f56567d238ed4198f2e9ef7de076d399980eae8dcdf84efe82 AS build
WORKDIR "/build"
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1

COPY src/Directory.Build.props ./src/

COPY src/FhirServerExporter/FhirServerExporter.csproj ./src/FhirServerExporter/
COPY src/FhirServerExporter/packages.lock.json ./src/FhirServerExporter/

RUN dotnet restore /p:ContinuousIntegrationBuild=true src/FhirServerExporter/FhirServerExporter.csproj

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

RUN dotnet restore src/FhirServerExporter.Tests/FhirServerExporter.Tests.csproj

WORKDIR /build/src/FhirServerExporter.Tests
RUN dotnet test \
    --configuration=Release \
    --collect:"XPlat Code Coverage" \
    --results-directory=./coverage \
    -l "console;verbosity=detailed"

FROM runtime
COPY --from=build /build/publish .
ENTRYPOINT ["dotnet", "FhirServerExporter.dll"]
