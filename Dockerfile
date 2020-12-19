FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /build
COPY src/FhirServerExporter/FhirServerExporter.csproj .
RUN dotnet restore
COPY . .

RUN dotnet publish \
    -c Release \
    -o /build/publish \
    src/FhirServerExporter/FhirServerExporter.csproj

FROM mcr.microsoft.com/dotnet/runtime:5.0-alpine
WORKDIR /opt
COPY --from=build /build/publish .

ARG VERSION=0.0.0
ENV APP__VERSION=${VERSION} \
    DOTNET_ENVIRONMENT="Production" \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
EXPOSE 9797
USER 65532
ENTRYPOINT ["dotnet", "/opt/FhirServerExporter.dll"]
