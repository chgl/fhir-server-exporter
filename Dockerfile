# kics false positive "Missing User Instruction": <https://docs.kics.io/latest/queries/dockerfile-queries/fd54f200-402c-4333-a5a4-36ef6709af2f/>
# kics-scan ignore-line
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/aspnet:10.0.10-resolute-chiseled@sha256:22467e5e67c226c1acbc8783e15ebecb3b0c0d8176c6c73109a630b1f4d87e33 AS runtime
WORKDIR /opt/fhir-server-exporter
EXPOSE 9797/tcp
# /home/app user. We can't use an id > 10_000 here since we need the home directory to install the duckdb extensions to.
USER 1654:1654
ENV ASPNETCORE_ENVIRONMENT="Production" \
    DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    ASPNETCORE_URLS="http://*:9797" \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0.302-resolute@sha256:c43f711c3ba4e621fe8570660f16abb31bbffda372fd2690eb917a85162a4281 AS build
WORKDIR "/build"
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
SHELL ["/bin/bash", "-eo", "pipefail", "-c"]

COPY src/Directory.Build.props ./src/

COPY src/FhirServerExporter/FhirServerExporter.csproj ./src/FhirServerExporter/
COPY src/FhirServerExporter/packages.lock.json ./src/FhirServerExporter/

# duckdb cli used to install the delta lake and s3 extensions.
# DuckDB.NET.Bindings.Full always mirrors the exact DuckDB core release it embeds natively (e.g.
# "1.4.4-alpha.2" / "1.1.0.1" still wrap duckdb v1.4.4 / v1.1.0), so the version to fetch here is
# derived from the pinned DuckDB.NET.Data.Full package reference below rather than tracked as a
# second, independently-drifting value. Otherwise the CLI-installed extensions land in a
# ~/.duckdb/extensions/<version>/ folder the app's embedded native lib never looks in.
RUN <<EOF
NUGET_VERSION=$(grep -oP 'DuckDB\.NET\.Data\.Full"\s+Version="\K[^"]+' src/FhirServerExporter/FhirServerExporter.csproj)
DUCKDB_VERSION=$(echo "$NUGET_VERSION" | cut -d'-' -f1 | cut -d'.' -f1-3)
echo "Resolved DuckDB core version $DUCKDB_VERSION from DuckDB.NET.Data.Full $NUGET_VERSION"
curl -LSs "https://github.com/duckdb/duckdb/releases/download/v${DUCKDB_VERSION}/duckdb_cli-linux-amd64.gz" | gunzip > duckdb
chmod +x ./duckdb
mv ./duckdb /usr/local/bin/duckdb
duckdb --version
duckdb -c "INSTALL delta; INSTALL httpfs; INSTALL aws;"
EOF

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
