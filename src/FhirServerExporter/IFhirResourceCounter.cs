using Hl7.Fhir.Model;

namespace FhirServerExporter;

public interface IFhirResourceCounter
{
    Task<long?> GetResourceCountAsync(
        ResourceType resourceType,
        CancellationToken cancellationToken = default
    );
}
