using System.Net.Http.Headers;
using System.Text;
using Duende.AccessTokenManagement;
using Microsoft.Extensions.Options;

namespace FhirServerExporter;

public interface IAuthHeaderProvider
{
    Task<AuthenticationHeaderValue?> GetAuthHeaderAsync(CancellationToken cancelToken = default);
}

public class AuthHeaderProvider : IAuthHeaderProvider
{
    public const string HttpClientName = "fhir.client";

    private readonly IClientCredentialsTokenManager tokenService;
    private readonly AuthConfig config;
    private readonly ILogger<AuthHeaderProvider> log;

    public AuthHeaderProvider(
        IOptions<AuthConfig> config,
        IClientCredentialsTokenManager tokenService,
        ILogger<AuthHeaderProvider> logger
    )
    {
        this.config = config.Value;
        log = logger;
        this.tokenService = tokenService;

        BasicAuthHeader = GetBasicAuthHeader();
        BearerTokenAuthHeader = GetBearerTokenHeader();
    }

    private AuthenticationHeaderValue? BasicAuthHeader { get; }

    private AuthenticationHeaderValue? BearerTokenAuthHeader { get; }

    public async Task<AuthenticationHeaderValue?> GetAuthHeaderAsync(
        CancellationToken cancelToken = default
    )
    {
        if (BasicAuthHeader is not null)
        {
            return await Task.FromResult(BasicAuthHeader);
        }

        if (BearerTokenAuthHeader is not null)
        {
            return await Task.FromResult(BearerTokenAuthHeader);
        }

        return await GetOAuthHeader(cancelToken);
    }

    private async Task<AuthenticationHeaderValue?> GetOAuthHeader(
        CancellationToken cancelToken = default
    )
    {
        var oAuthUri = config.OAuth.TokenUrl;
        if (oAuthUri is null)
        {
            return null;
        }

        var token = await tokenService.GetAccessTokenAsync(
            ClientCredentialsClientName.Parse(HttpClientName),
            ct: cancelToken
        );
        if (token.Token?.AccessToken is null)
        {
            log.LogError("Failed to get oauth token.");
            return null;
        }

        return new AuthenticationHeaderValue("Bearer", token.Token.AccessToken);
    }

    private AuthenticationHeaderValue? GetBasicAuthHeader()
    {
        var username = config.Basic.Username;
        var password = config.Basic.Password;

        if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
        {
            var byteArray = Encoding.ASCII.GetBytes($"{username}:{password}");
            log.LogDebug("Using basic auth");
            return new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }

        return null;
    }

    private AuthenticationHeaderValue? GetBearerTokenHeader()
    {
        var bearerTokenFromConfig = config.BearerToken;

        if (!string.IsNullOrWhiteSpace(bearerTokenFromConfig))
        {
            log.LogDebug("Using static bearer token");
            return new AuthenticationHeaderValue("Bearer", bearerTokenFromConfig);
        }

        return null;
    }
}

public interface IFhirResourceCounter
{
    Task<IReadOnlyDictionary<string, int>> CountResourcesAsync(CancellationToken cancellationToken = default);
}

public class FhirResourceCounter : IFhirResourceCounter
{
    private readonly System.Net.Http.HttpClient httpClient;
    private readonly Microsoft.Extensions.Logging.ILogger<FhirResourceCounter> log;
    private readonly Uri serverUrl;
    private readonly string[] resourceTypes;
    private readonly Dictionary<string, int> counts = new();

    public FhirResourceCounter(
        Microsoft.Extensions.Options.IOptions<AppConfig> config,
        Microsoft.Extensions.DependencyInjection.IHttpClientFactory clientFactory,
        Microsoft.Extensions.Logging.ILogger<FhirResourceCounter> log
    )
    {
        this.httpClient = clientFactory.CreateClient(AuthHeaderProvider.HttpClientName);
        this.log = log;
        serverUrl = config.Value.FhirServerUrl;
        resourceTypes = GetResourceTypes(config.Value);
    }

    public async Task<IReadOnlyDictionary<string, int>> CountResourcesAsync(CancellationToken cancellationToken = default)
    {
        counts.Clear();
        const int batchSize = 100;
        var pending = new Queue<string>(resourceTypes);
        while (pending.Count > 0)
        {
            var batch = new List<string>(Math.Min(batchSize, pending.Count));
            for (var i = 0; i < batch.Capacity; i++)
            {
                batch.Add(pending.Dequeue());
            }

            var payload = new
            {
                resourceType = "Bundle",
                type = "search",
                entry = batch.Select(t => new { url = $"{t}?_summary=count" })
            };

            using var content = new System.Net.Http.StringContent(
                System.Text.Json.JsonSerializer.Serialize(payload),
                System.Text.Encoding.UTF8,
                "application/fhir+json"
            );

            try
            {
                using var response = await httpClient.PostAsync(serverUrl, content, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    log.LogWarning("Search bundle returned {StatusCode}", (int)response.StatusCode);
                    continue;
                }

                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                using var document = System.Text.Json.JsonDocument.Parse(body);
                if (!document.RootElement.TryGetProperty("entry", out var entries) || entries.ValueKind != System.Text.Json.JsonValueKind.Array)
                {
                    continue;
                }

                var remaining = new List<string>(batch);
                foreach (var entry in entries.EnumerateArray())
                {
                    string? url = null;
                    if (entry.TryGetProperty("request", out var request)
                        && request.ValueKind == System.Text.Json.JsonValueKind.Object
                        && request.TryGetProperty("url", out var requestUrl)
                        && requestUrl.ValueKind == System.Text.Json.JsonValueKind.String)
                    {
                        url = requestUrl.GetString();
                    }

                    int count = 0;
                    if (entry.TryGetProperty("resource", out var resource)
                        && resource.ValueKind == System.Text.Json.JsonValueKind.Object
                        && resource.TryGetProperty("total", out var total)
                        && total.ValueKind == System.Text.Json.JsonValueKind.Number)
                    {
                        count = total.GetInt32();
                    }

                    var type = GetResourceType(url);
                    if (type is null || !remaining.Contains(type))
                    {
                        if (remaining.Count > 0)
                        {
                            type = remaining[0];
                        }
                    }

                    if (type is not null)
                    {
                        counts[type] = count;
                        remaining.Remove(type);
                    }
                }

                foreach (var type in remaining)
                {
                    counts[type] = 0;
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Failed to fetch search bundle");
            }
        }

        return counts;
    }

    private static string[] GetResourceTypes(AppConfig config)
    {
        if (!string.IsNullOrWhiteSpace(config.IncludedResources))
        {
            return config.IncludedResources.Split(',', System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries).ToArray();
        }

        var all = GetDefaultResourceTypes();
        if (!string.IsNullOrWhiteSpace(config.ExcludedResources))
        {
            var excluded = new HashSet<string>(config.ExcludedResources.Split(',', System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries), StringComparer.OrdinalIgnoreCase);
            return all.Where(t => !excluded.Contains(t)).ToArray();
        }

        return all;
    }

    private static string[] GetDefaultResourceTypes()
    {
        var type = Type.GetType("Hl7.Fhir.Model.FhirResourceType, Hl7.Fhir.Model");
        if (type is null)
        {
            return Array.Empty<string>();
        }

        return Enum.GetNames(type);
    }

    private static string? GetResourceType(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        var path = url.Split('?')[0].TrimEnd('/');
        var segments = path.Split('/', System.StringSplitOptions.RemoveEmptyEntries);

        return segments.Length > 0 ? segments[^1] : null;
    }
}
