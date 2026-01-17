using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;

namespace FhirServerExporter;

public class FhirServerResourceCounter(
    FhirClient fhirClient,
    IAuthHeaderProvider authHeaderProvider
) : IFhirResourceCounter
{
    private readonly FhirClient fhirClient = fhirClient;
    private readonly IAuthHeaderProvider authHeaderProvider = authHeaderProvider;

    public async Task<long?> GetResourceCountAsync(
        ResourceType resourceType,
        CancellationToken cancellationToken = default
    )
    {
        fhirClient.RequestHeaders!.Authorization = await authHeaderProvider.GetAuthHeaderAsync(
            cancellationToken
        );

        var response = await fhirClient.SearchAsync(
            resourceType.ToString(),
            summary: SummaryType.Count,
            ct: cancellationToken
        );
        return response?.Total;
    }
}
