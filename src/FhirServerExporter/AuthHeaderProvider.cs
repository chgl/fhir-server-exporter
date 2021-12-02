using System;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.AspNetCore.AccessTokenManagement;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public interface IAuthHeaderProvider
{
    Task<AuthenticationHeaderValue> GetAuthHeaderAsync(CancellationToken cancelToken = default);
}

public class AuthHeaderProvider : IAuthHeaderProvider
{
    private readonly IClientAccessTokenManagementService tokenService;
    private readonly IConfiguration config;
    private readonly ILogger<AuthHeaderProvider> log;

    public AuthHeaderProvider(IConfiguration config, IClientAccessTokenManagementService tokenService, ILogger<AuthHeaderProvider> logger)
    {
        this.config = config;
        log = logger;
        this.tokenService = tokenService;

        BasicAuthHeader = GetBasicAuthHeader();
        BearerTokenAuthHeader = GetBearerTokenHeader();
    }

    public AuthenticationHeaderValue BasicAuthHeader { get; }

    public AuthenticationHeaderValue BearerTokenAuthHeader { get; }

    public async Task<AuthenticationHeaderValue> GetAuthHeaderAsync(CancellationToken cancelToken = default)
    {
        if (BasicAuthHeader != null)
        {
            return await Task.FromResult(BasicAuthHeader);
        }

        if (BearerTokenAuthHeader != null)
        {
            return await Task.FromResult(BearerTokenAuthHeader);
        }

        return await GetOAuthHeader(cancelToken);
    }

    private async Task<AuthenticationHeaderValue> GetOAuthHeader(CancellationToken cancelToken = default)
    {
        var oAuthUri = config.GetValue<Uri>("Auth:OAuth:TokenUrl");
        if (oAuthUri != null)
        {
            var token = await tokenService.GetClientAccessTokenAsync(cancellationToken: cancelToken);

            if (token == null)
            {
                log.LogError("Failed to get oauth token.");
                return null;
            }

            log.LogDebug("Using oauth token");

            return new AuthenticationHeaderValue("Bearer", token);
        }

        return null;
    }

    private AuthenticationHeaderValue GetBasicAuthHeader()
    {
        var username = config.GetValue<string>("Auth:Basic:Username", null);
        var password = config.GetValue<string>("Auth:Basic:Password", null);

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
        var bearerTokenFromConfig = config.GetValue<string>("Auth:BearerToken");

        if (!string.IsNullOrWhiteSpace(bearerTokenFromConfig))
        {
            log.LogDebug("Using static bearer token");
            return new AuthenticationHeaderValue("Bearer", bearerTokenFromConfig);
        }

        return null;
    }
}
