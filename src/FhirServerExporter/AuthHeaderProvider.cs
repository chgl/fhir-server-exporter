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

    private readonly IClientCredentialsTokenManagementService tokenService;
    private readonly AuthConfig config;
    private readonly ILogger<AuthHeaderProvider> log;

    public AuthHeaderProvider(
        IOptions<AuthConfig> config,
        IClientCredentialsTokenManagementService tokenService,
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
            HttpClientName,
            cancellationToken: cancelToken
        );
        if (token is null)
        {
            log.LogError("Failed to get oauth token.");
            return null;
        }

        return new AuthenticationHeaderValue("Bearer", token.AccessToken);
    }

    private AuthenticationHeaderValue? GetBasicAuthHeader()
    {
        var username = config.Basic.Username;
        var password = config.Basic.Password;

        if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(username))
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
