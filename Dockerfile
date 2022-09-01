# syntax=docker/dockerfile:1.4
FROM mcr.microsoft.com/dotnet/nightly/sdk:6.0.400-jammy@sha256:01fc2d5af703fb0c38261ef67e395ac07d783bc476fc98c7d78d4c896994bbd2 AS build
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

FROM mcr.microsoft.com/dotnet/nightly/aspnet:6.0.8-jammy-chiseled@sha256:5258a1139db036d151e49934406c50fc41604b9519441294e017d75347f932e6
WORKDIR /opt/fhir-server-exporter
ENV DOTNET_ENVIRONMENT="Production" \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
EXPOSE 9797/tcp
USER 65532:65532

COPY --from=build /build/publish .

ENTRYPOINT ["dotnet", "FhirServerExporter.dll"]
