FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /build
COPY src/FhirServerExporter/FhirServerExporter.csproj .
COPY src/FhirServerExporter/packages.lock.json .
RUN dotnet restore --locked-mode
COPY . .

RUN dotnet publish \
    -c Release \
    -o /build/publish \
    src/FhirServerExporter/FhirServerExporter.csproj

FROM build AS test
WORKDIR /build/src/FhirServerExporter.Tests
RUN dotnet test -p:CollectCoverage=true

FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine
WORKDIR /opt/fhir-server-exporter
COPY --from=build /build/publish .

ENV DOTNET_ENVIRONMENT="Production" \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
EXPOSE 9797
USER 65532
ENTRYPOINT ["dotnet", "/opt/fhir-server-exporter/FhirServerExporter.dll"]
