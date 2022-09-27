# syntax=docker/dockerfile:1.4
FROM mcr.microsoft.com/dotnet/nightly/sdk:6.0.401-jammy@sha256:f68325aecf05364c1c8ca7582d9b9bc7c39cfc2b341b1b67d0e9e911f93ab445 AS build
WORKDIR "/build"

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
RUN dotnet test src/FhirServerExporter.Tests/FhirServerExporter.Tests.csproj \
    --no-restore \
    -p:CollectCoverage=true

FROM mcr.microsoft.com/dotnet/nightly/aspnet:6.0.9-jammy-chiseled@sha256:064f335b86b3e8cafbafafcb35d20f566a823b139a4261624f7f1f8b93dfb7f7
WORKDIR /opt/fhir-server-exporter
ENV DOTNET_ENVIRONMENT="Production" \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
EXPOSE 9797/tcp
USER 65532:65532

COPY --from=build /build/publish .

ENTRYPOINT ["dotnet", "FhirServerExporter.dll"]
