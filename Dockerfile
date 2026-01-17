# kics false positive "Missing User Instruction": <https://docs.kics.io/latest/queries/dockerfile-queries/fd54f200-402c-4333-a5a4-36ef6709af2f/>
# kics-scan ignore-line
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/aspnet:10.0.1-noble-chiseled@sha256:ba111738d21b4898f433fd8724ba1ed2e450adcb295c58f31d4137751922c83c AS runtime
WORKDIR /opt/fhir-server-exporter
EXPOSE 9797/tcp
# /home/app user. We can't use an id > 10_000 here since we need the home directory to install the duckdb extensions to.
USER 1654:1654
ENV ASPNETCORE_ENVIRONMENT="Production" \
    DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    ASPNETCORE_URLS="http://*:9797" \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0.101-noble@sha256:5504edd1267dd4deab3443f960cfab219249c8bd935fbcc358f1c24aeae23fe0 AS build
WORKDIR "/build"
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
SHELL ["/bin/bash", "-eo", "pipefail", "-c"]

# duckdb cli used to install the delta lake and s3 extensions
# renovate: datasource=github-releases depName=duckdb/duckdb
ARG DUCKDB_VERSION=1.4.3
ENV DUCKDB_URL="https://github.com/duckdb/duckdb/releases/download/v${DUCKDB_VERSION}/duckdb_cli-linux-amd64.gz"
RUN <<EOF
curl -LSs "$DUCKDB_URL" | gunzip > duckdb
chmod +x ./duckdb
mv ./duckdb /usr/local/bin/duckdb
duckdb --version
duckdb -c "INSTALL delta; INSTALL httpfs; INSTALL aws;"
EOF

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
COPY --from=build --chown=1654:1654 /root/.duckdb/extensions/ /home/app/.duckdb/extensions/
COPY --from=build /build/publish .
ENTRYPOINT ["dotnet", "FhirServerExporter.dll"]
