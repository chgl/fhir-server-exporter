# FHIR Server Exporter

FHIR server resource count exporter for Prometheus.

## Usage

```sh
docker run --rm -it \
    -p 9797:9797 \
    -e FHIRSERVERURL="https://hapi.fhir.org/baseR4" \
    -e FETCHINTERVALSECONDS=60 \
    quay.io/chgl/fhir-server-exporter:latest
```

Open <http://localhost:9797/metrics> to view the resource counts in Prometheus format:

```console
# HELP fhir_resource_count Number of resources stored within the FHIR server by type.
# TYPE fhir_resource_count gauge
fhir_resource_count{type="Patient"} 124005
fhir_resource_count{type="Condition"} 29282
fhir_resource_count{type="DiagnosticReport"} 36429
...
```

The container image is pushed to three registries:

- `quay.io/chgl/fhir-server-exporter:latest`
- `docker.io/chgl/fhir-server-exporter:latest`
- `ghcr.io/chgl/fhir-server-exporter:latest`

You are strongly encouraged to use the `vX.Y.Z` tags corresponding to the [releases](https://github.com/chgl/fhir-server-exporter/releases)
instead of using `latest`.

### Configuration

| Environment Variable      | Description                                                                                                     | Default value |
| ------------------------- | --------------------------------------------------------------------------------------------------------------- | ------------- |
| FHIRSERVERURL             | The base URL of the FHIR server whose metrics should be exported. E.g. `http://localhost:8082/fhir`             | `""`          |
| FETCHINTERVALSECONDS      | The number of seconds between consecutive REST requests to the FHIR server to fetch all resource counts.        | `30`          |
| METRICSPORT               | The local port on which the metrics should be exposed at.                                                       | `9797`        |
| EXCLUDEDRESOURCES         | A comma-seperated list of FHIR resource types that should be excluded from counting. E.g. `Binary,Subscription` | `""`          |
| AUTH\_\_BASIC\_\_USERNAME | If the FHIR server requires basic auth, this allows setting the username.                                       | `""`          |
| AUTH\_\_BASIC\_\_PASSWORD | Basic auth password.                                                                                            | `""`          |

## Install on Kubernetes

To deploy the exporter on Kubernetes, a Helm chart is available at <https://github.com/chgl/charts/tree/master/charts/fhir-server-exporter>.

## Development

1. Start an empty HAPI FHIR server exposed on port 8282 and a pre-configured Prometheus instance on port 9090:

   ```sh
   docker-compose -f hack/docker-compose.dev.yml up
   ```

1. Run the server exporter

   ```sh
   cd src/FhirServerExporter/
   dotnet run
   ```

1. Access the exposed metrics at <http://localhost:9797/metrics>

## Build and run container image locally

```sh
docker build -t fhir-server-exporter .
docker run -e FHIRSERVERURL="http://host.docker.internal:8082/fhir" -p 9797:9797 fhir-server-exporter
```
