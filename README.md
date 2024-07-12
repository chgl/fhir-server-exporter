# FHIR Server Exporter

![Last Version](https://img.shields.io/github/v/release/chgl/fhir-server-exporter)
![License](https://img.shields.io/github/license/chgl/fhir-server-exporter)
![Docker Pull](https://img.shields.io/docker/pulls/chgl/fhir-server-exporter)
[![CI](https://github.com/chgl/fhir-server-exporter/actions/workflows/ci.yaml/badge.svg)](https://github.com/chgl/fhir-server-exporter/actions/workflows/ci.yaml)
[![OpenSSF Scorecard](https://api.securityscorecards.dev/projects/github.com/chgl/fhir-server-exporter/badge)](https://api.securityscorecards.dev/projects/github.com/chgl/fhir-server-exporter)
[![SLSA 3](https://slsa.dev/images/gh-badge-level3.svg)](https://slsa.dev)

FHIR server resource count exporter for Prometheus.

## Usage

```sh
docker run --rm -it \
    -p 9797:9797 \
    -e FhirServerUrl="https://hapi.fhir.org/baseR4" \
    -e FetchIntervalSeconds=60 \
    -e FhirServerName="HAPI FHIR Demo Server" \
    ghcr.io/chgl/fhir-server-exporter:v2.3.6
```

Open <http://localhost:9797/metrics> to view the resource counts in Prometheus format:

```console
# HELP fhir_resource_count Number of resources stored within the FHIR server by type.
# TYPE fhir_resource_count gauge
fhir_resource_count{type="Patient", server_name="HAPI FHIR Demo Server"} 124005
fhir_resource_count{type="Condition", server_name="HAPI FHIR Demo Server"} 29282
fhir_resource_count{type="DiagnosticReport", server_name="HAPI FHIR Demo Server"} 36429
...
```

The container image is pushed to these registries:

- docker.io/chgl/fhir-server-exporter:v2.3.6
- ghcr.io/chgl/fhir-server-exporter:v2.3.6

### Configuration

| Environment Variable          | Description                                                                                                                                                               | Default value  |
| ----------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------- |
| FhirServerUrl                 | The base URL of the FHIR server whose metrics should be exported. E.g. `http://localhost:8082/fhir`                                                                       | `""`           |
| FhirServerName                | A friendly name for the server. Included as a `server_name` label in the `fhir_resource_count` metric.                                                                    | `""`           |
| FetchIntervalSeconds          | The number of seconds between consecutive REST requests to the FHIR server to fetch all resource counts.                                                                  | `30`           |
| MetricsPort                   | The local port on which the metrics should be exposed at.                                                                                                                 | `9797`         |
| FhirServerTimeout             | The HTTP client timeout for querying the FHIR server in [TimeSpan](https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-timespan-format-strings) format. | `"0.00:02:00"` |
| ExcludedResources             | A comma-separated list of FHIR resource types that should be excluded from counting. E.g. `Binary,Subscription`                                                           | `""`           |
| IncludedResources             | A comma-separated list of FHIR resource types that should be included for counting. if unset, defaults to all types.                                                      | `""`           |
| Auth\_\_Basic\_\_Username     | If the FHIR server requires basic auth, this allows setting the username.                                                                                                 | `""`           |
| Auth\_\_Basic\_\_Password     | Basic auth password.                                                                                                                                                      | `""`           |
| Auth\_\_BearerToken           | Static token to set in the `Authorization: Bearer …` header.                                                                                                              | `""`           |
| Auth\_\_OAuth\_\_TokenUrl     | OAuth token endpoint URL.                                                                                                                                                 | `""`           |
| Auth\_\_OAuth\_\_ClientId     | OAuth client ID.                                                                                                                                                          | `""`           |
| Auth\_\_OAuth\_\_ClientSecret | OAuth client secret                                                                                                                                                       | `""`           |
| Auth\_\_OAuth\_\_Scope        | OAuth scope                                                                                                                                                               | `""`           |

### Custom Queries

You can also specify a list of custom queries to run against the FHIR server.
Create a file called `queries.yaml` and place it in the application's main directory:

```sh
docker run --rm -it \
   -e FhirServerUrl="https://hapi.fhir.org/baseR4" \
   -e FhirServerName="HAPI FHIR Demo Server" \
   -p 9797:9797 \
   -v $PWD/src/FhirServerExporter/queries.yaml:/opt/fhir-server-exporter/queries.yaml:ro \
   ghcr.io/chgl/fhir-server-exporter:v2.3.6
```

Here's an example `queries.yaml` file. It exports three gauge metrics as `fhir_male_patient_count`,
`fhir_female_patient_count`, and `fhir_older_female_patient_count`.

```yaml
queries:
  - name: fhir_male_patient_count
    query: Patient?gender=male
    description: Male patients
  - name: fhir_female_patient_count
    query: Patient?gender=female
  - name: fhir_older_female_patient_count
    query: Patient?gender=female&birthdate=le1900
    description: Female patients born on or before 1900
```

Note that `&_summary=count` is automatically appended to the query.

## Install on Kubernetes

To deploy the exporter on Kubernetes, a Helm chart is available at <https://github.com/chgl/charts/tree/master/charts/fhir-server-exporter>.

## Authentication

If multiple authentication settings are given, the order of usage is:

1. Basic Auth
1. Bearer Token
1. OAuth

so if you've specified both a basic auth username and password and an oauth token URL, only the basic auth is used.

## Development

### Using Docker Compose

1. Start an empty HAPI FHIR server exposed on port 8282 and a pre-configured Prometheus instance on port 9090:

   ```sh
   docker compose -f hack/compose.yaml up
   ```

1. Run the server exporter

   ```sh
   cd src/FhirServerExporter/
   dotnet run
   ```

1. Access the exposed metrics at <http://localhost:9797/metrics>

### On Kubernetes using Skaffold+Kustomize

1. create a local testing cluster

   ```sh
   kind create cluster
   ```

1. build and deploy container in development mode. This also bootstraps a HAPI FHIR server and loads some sample resources into it.

   ```sh
   skaffold dev --port-forward
   ```

### Build and run container image locally

```sh
docker build -t fhir-server-exporter:local .
docker run -e FhirServerUrl="http://host.docker.internal:8082/fhir" -p 9797:9797 fhir-server-exporter:local
```

## Image signature and provenance verification

Prerequisites:

- [cosign](https://github.com/sigstore/cosign/releases)
- [slsa-verifier](https://github.com/slsa-framework/slsa-verifier/releases)
- [crane](https://github.com/google/go-containerregistry/releases)

All released container images are signed using [cosign](https://github.com/sigstore/cosign) and SLSA Level 3 provenance is available for verification.

```sh
IMAGE=ghcr.io/chgl/fhir-server-exporter:v2.3.6
DIGEST=$(crane digest "${IMAGE}")
IMAGE_DIGEST_PINNED="ghcr.io/chgl/fhir-server-exporter@${DIGEST}"
IMAGE_TAG="${IMAGE#*:}"

cosign verify \
   --certificate-oidc-issuer="https://token.actions.githubusercontent.com" \
   --certificate-identity-regexp="^https://github.com/chgl/.github/.github/workflows/standard-build.yaml@[0-9a-f]{40}$" \
   --certificate-github-workflow-repository="chgl/fhir-server-exporter" \
   "${IMAGE_DIGEST_PINNED}"

slsa-verifier verify-image \
    --source-uri github.com/chgl/fhir-server-exporter \
    --source-tag ${IMAGE_TAG} \
    --source-branch master \
    "${IMAGE_DIGEST_PINNED}"
```
