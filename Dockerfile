FROM mcr.microsoft.com/dotnet/sdk:6.0-bullseye-slim-amd64 AS build
WORKDIR /build
COPY src/FhirServerExporter/FhirServerExporter.csproj .
RUN dotnet restore
COPY . .

RUN dotnet publish \
    -c Release \
    -o /build/publish \
    /p:UseAppHost=false \
    src/FhirServerExporter/FhirServerExporter.csproj

FROM build AS test
WORKDIR /build/src/FhirServerExporter.Tests
RUN dotnet test -p:CollectCoverage=true

FROM mcr.microsoft.com/dotnet/aspnet:6.0-bullseye-slim
WORKDIR /opt/fhir-server-exporter
COPY --from=build /build/publish .

ENV DOTNET_ENVIRONMENT="Production" \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
EXPOSE 9797
USER 65532
ENTRYPOINT ["dotnet", "/opt/fhir-server-exporter/FhirServerExporter.dll"]
