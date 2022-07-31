# syntax=docker/dockerfile:1.4
FROM mcr.microsoft.com/dotnet/sdk:6.0-bullseye-slim-amd64 as build
WORKDIR "/build"
COPY src/FhirServerExporter.Tests/FhirServerExporter.Tests.csproj ./src/FhirServerExporter.Tests/
COPY src/FhirServerExporter/FhirServerExporter.csproj ./src/FhirServerExporter/

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore src/FhirServerExporter/FhirServerExporter.csproj \
    --runtime linux-x64

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore src/FhirServerExporter.Tests/FhirServerExporter.Tests.csproj\
    --runtime linux-x64

COPY . .

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet build src/FhirServerExporter/FhirServerExporter.csproj \
    --no-restore \
    --configuration Release \
    --runtime linux-x64 \
    --self-contained true \
    --framework net6.0

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish src/FhirServerExporter/FhirServerExporter.csproj \
    --no-restore \
    --no-build \
    --configuration Release \
    --runtime linux-x64 \
    --self-contained true \
    --framework net6.0 \
    -o /build/publish

FROM build AS test
WORKDIR /build/src/FhirServerExporter.Tests
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet test FhirServerExporter.Tests.csproj \
    --no-restore \
    -p:CollectCoverage=true

FROM gcr.io/distroless/cc-debian11:nonroot@sha256:10798c5d3c3cee4d740037fbf932a8c73d0b920afd5ba5b3d4acd9ae05565b50
WORKDIR /opt/fhir-server-exporter
ENV DOTNET_ENVIRONMENT="Production" \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
EXPOSE 9797/tcp
USER 65532:65532

COPY --from=build /lib/x86_64-linux-gnu/libz.so.1.2.11 /lib/x86_64-linux-gnu/libz.so.1
COPY --from=build /build/publish .

ENTRYPOINT ["/opt/fhir-server-exporter/FhirServerExporter"]
