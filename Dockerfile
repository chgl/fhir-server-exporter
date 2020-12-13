FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /build
COPY src/FhirServerExporter/FhirServerExporter.csproj .
RUN dotnet restore
COPY . .

# if PublishTrimmed=true image size is reduced by ~30%
# but build time is nearly doubled
RUN dotnet publish src/FhirServerExporter/FhirServerExporter.csproj \
    -c Release \
    -r linux-musl-x64 \
    --self-contained true \
    -p:PublishSingleFile=false \
    -p:PublishTrimmed=true \
    -o /build/publish

FROM mcr.microsoft.com/dotnet/runtime:5.0-alpine
WORKDIR /opt
COPY --from=build /build/publish .

ARG VERSION=0.0.0
ENV APP__VERSION=${VERSION} \
    DOTNET_ENVIRONMENT="Production" \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
EXPOSE 9797
USER 65532
ENTRYPOINT ["/opt/FhirServerExporter"]
