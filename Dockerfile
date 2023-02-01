# syntax=docker/dockerfile:1.4
FROM mcr.microsoft.com/dotnet/nightly/aspnet:7.0-jammy-chiseled@sha256:c8eeae2f70f6f4bb83c1863edaa8c23a2d4877cac46dc131619a3b5fbddf780a AS runtime
WORKDIR /opt/fhir-server-exporter
EXPOSE 9797/tcp
USER 65532:65532
ENV ASPNETCORE_ENVIRONMENT="Production" \
    DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    ASPNETCORE_URLS="http://*:9797" \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

FROM mcr.microsoft.com/dotnet/sdk:7.0-jammy@sha256:1f03f2609039c9172458f1172e4646032f1894fbc8e0cc6dfdfa5373348ae6cc AS build
WORKDIR "/build"
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1

COPY src/FhirServerExporter.Tests/FhirServerExporter.Tests.csproj ./src/FhirServerExporter.Tests/
COPY src/FhirServerExporter/FhirServerExporter.csproj ./src/FhirServerExporter/

RUN dotnet restore src/FhirServerExporter/FhirServerExporter.csproj

RUN dotnet restore src/FhirServerExporter.Tests/FhirServerExporter.Tests.csproj

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
WORKDIR /build/src/FhirServerExporter.Tests
RUN dotnet test \
    --configuration=Release \
    --collect:"XPlat Code Coverage" \
    --results-directory=./coverage \
    -l "console;verbosity=detailed"

FROM runtime
COPY --from=build /build/publish .
ENTRYPOINT ["dotnet", "FhirServerExporter.dll"]
