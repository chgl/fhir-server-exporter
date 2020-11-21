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

Open <http://localhost:9797/metrics> to view the resource counts in Prometheus format and configure your [prometheus.yml](/hack/prometheus.yml) accordingly.

## Configuration

| Environment Variable | Description                                                                                                     | Default value |
| -------------------- | --------------------------------------------------------------------------------------------------------------- | ------------- |
| FHIRSERVERURL        | The base URL of the FHIR server whose metrics should be exported. E.g. `http://localhost:8082/fhir`             | ``            |
| FETCHINTERVALSECONDS | The number of seconds between consecutive REST requests to the FHIR server to fetch all resource counts.        | `30`          |
| METRICSPORT          | The local port on which the metrics should be exposed at.                                                       | `9797`        |
| EXCLUDEDRESOURCES    | A comma-seperated list of FHIR resource types that should be excluded from counting. E.g. `Binary,Subscription` | ``            |
