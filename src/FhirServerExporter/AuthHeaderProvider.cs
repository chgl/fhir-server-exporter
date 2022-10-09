using System;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.AspNetCore.AccessTokenManagement;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public interface IAuthHeaderProvider
{
    Task<AuthenticationHeaderValue> GetAuthHeaderAsync(CancellationToken cancelToken = default);
}

public class AuthHeaderProvider : IAuthHeaderProvider
{
    private readonly IClientAccessTokenManagementService tokenService;
    private readonly AuthConfig config;
    private readonly ILogger<AuthHeaderProvider> log;

    public AuthHeaderProvider(IOptions<AuthConfig> config, IClientAccessTokenManagementService tokenService, ILogger<AuthHeaderProvider> logger)
    {
        this.config = config.Value;
        log = logger;
        this.tokenService = tokenService;

        BasicAuthHeader = GetBasicAuthHeader();
        BearerTokenAuthHeader = GetBearerTokenHeader();
    }

    private AuthenticationHeaderValue BasicAuthHeader { get; }

    private AuthenticationHeaderValue BearerTokenAuthHeader { get; }

    public async Task<AuthenticationHeaderValue> GetAuthHeaderAsync(CancellationToken cancelToken = default)
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

    private async Task<AuthenticationHeaderValue> GetOAuthHeader(CancellationToken cancelToken = default)
    {
        var oAuthUri = config.OAuth.TokenUrl;
        if (oAuthUri is null)
        {
            return null;
        }

        var token = await tokenService.GetClientAccessTokenAsync(cancellationToken: cancelToken);
        if (token is null)
        {
            log.LogError("Failed to get oauth token.");
            return null;
        }

        return new AuthenticationHeaderValue("Bearer", token);
    }

    private AuthenticationHeaderValue GetBasicAuthHeader()
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

    private AuthenticationHeaderValue GetBearerTokenHeader()
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
