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
# HELP process_working_set_bytes Process working set
# TYPE process_working_set_bytes gauge
process_working_set_bytes 115916800
# HELP process_open_handles Number of open handles
# TYPE process_open_handles gauge
process_open_handles 312
# HELP fhir_resource_count Number of resources stored within the FHIR server by type.
# TYPE fhir_resource_count gauge
fhir_resource_count{type="ImmunizationEvaluation"} 0
fhir_resource_count{type="Basic"} 0
fhir_resource_count{type="MedicinalProductUndesirableEffect"} 0
fhir_resource_count{type="CapabilityStatement"} 0
fhir_resource_count{type="DiagnosticReport"} 0
fhir_resource_count{type="Goal"} 0
fhir_resource_count{type="SubstanceNucleicAcid"} 0
fhir_resource_count{type="Library"} 0
fhir_resource_count{type="MedicinalProduct"} 0
fhir_resource_count{type="VisionPrescription"} 0
fhir_resource_count{type="GraphDefinition"} 0
fhir_resource_count{type="ImplementationGuide"} 0
fhir_resource_count{type="CareTeam"} 0
fhir_resource_count{type="EffectEvidenceSynthesis"} 0
fhir_resource_count{type="PractitionerRole"} 0
fhir_resource_count{type="DeviceDefinition"} 0
fhir_resource_count{type="ClinicalImpression"} 0
fhir_resource_count{type="InsurancePlan"} 0
fhir_resource_count{type="MedicinalProductInteraction"} 0
fhir_resource_count{type="MedicinalProductIndication"} 0
fhir_resource_count{type="AdverseEvent"} 0
fhir_resource_count{type="ExampleScenario"} 0
fhir_resource_count{type="MedicinalProductContraindication"} 0
...
```

### Configuration

| Environment Variable      | Description                                                                                                     | Default value |
| ------------------------- | --------------------------------------------------------------------------------------------------------------- | ------------- |
| FHIRSERVERURL             | The base URL of the FHIR server whose metrics should be exported. E.g. `http://localhost:8082/fhir`             | `""`          |
| FETCHINTERVALSECONDS      | The number of seconds between consecutive REST requests to the FHIR server to fetch all resource counts.        | `30`          |
| METRICSPORT               | The local port on which the metrics should be exposed at.                                                       | `9797`        |
| EXCLUDEDRESOURCES         | A comma-seperated list of FHIR resource types that should be excluded from counting. E.g. `Binary,Subscription` | `""`          |
| AUTH\_\_BASIC\_\_USERNAME | If the FHIR server requires basic auth, this allows setting the username.                                       | `""`          |
| AUTH\_\_BASIC\_\_PASSWORD | Basic auth password.                                                                                            | `""`          |

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
