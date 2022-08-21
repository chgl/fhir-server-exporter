# syntax=docker/dockerfile:1.4
FROM mcr.microsoft.com/dotnet/nightly/sdk:6.0.400-jammy@sha256:7910472f0e7202c6a18ff5671182bbbe261abcab532fc30389b980fadff58091 AS build
WORKDIR "/build"

COPY src/FhirServerExporter.Tests/FhirServerExporter.Tests.csproj ./src/FhirServerExporter.Tests/
COPY src/FhirServerExporter/FhirServerExporter.csproj ./src/FhirServerExporter/

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore src/FhirServerExporter/FhirServerExporter.csproj

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore src/FhirServerExporter.Tests/FhirServerExporter.Tests.csproj

COPY . .

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet build src/FhirServerExporter/FhirServerExporter.csproj \
    --no-restore \
    --configuration Release

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish src/FhirServerExporter/FhirServerExporter.csproj \
    --no-restore \
    --no-build \
    --configuration Release \
    -o /build/publish

FROM build AS test
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet test src/FhirServerExporter.Tests/FhirServerExporter.Tests.csproj \
    --no-restore \
    -p:CollectCoverage=true

FROM mcr.microsoft.com/dotnet/nightly/aspnet:6.0.8-jammy-chiseled@sha256:5258a1139db036d151e49934406c50fc41604b9519441294e017d75347f932e6
WORKDIR /opt/fhir-server-exporter
ENV DOTNET_ENVIRONMENT="Production" \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
EXPOSE 9797/tcp
USER 65532:65532

COPY --from=build /build/publish .

ENTRYPOINT ["dotnet", "FhirServerExporter.dll"]
