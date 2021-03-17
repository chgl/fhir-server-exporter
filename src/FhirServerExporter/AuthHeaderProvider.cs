using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Configuration;
using System;
using Microsoft.Extensions.Logging;
using IdentityModel.AspNetCore.AccessTokenManagement;
using System.Threading;
using System.Threading.Tasks;

public interface IAuthHeaderProvider
{
    Task<AuthenticationHeaderValue> GetAuthHeaderAsync(CancellationToken cancelToken = default);
}

public class AuthHeaderProvider : IAuthHeaderProvider
{
    private readonly IClientAccessTokenManagementService _tokenService;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthHeaderProvider> _log;

    public AuthHeaderProvider(IConfiguration config, IClientAccessTokenManagementService tokenService, ILogger<AuthHeaderProvider> logger)
    {
        _config = config;
        _log = logger;
        _tokenService = tokenService;

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
        var oAuthUri = _config.GetValue<Uri>("Auth:OAuth:TokenUrl");
        if (oAuthUri != null)
        {
            var token = await _tokenService.GetClientAccessTokenAsync(cancellationToken: cancelToken);

            if (token == null)
            {
                _log.LogError("Failed to get oauth token.");
                return null;
            }

            _log.LogDebug("Using oauth token");

            return new AuthenticationHeaderValue("Bearer", token);
        }

        return null;
    }

    private AuthenticationHeaderValue GetBasicAuthHeader()
    {
        var username = _config.GetValue<string>("Auth:Basic:Username", null);
        var password = _config.GetValue<string>("Auth:Basic:Password", null);

        if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(username))
        {
            var byteArray = Encoding.ASCII.GetBytes($"{username}:{password}");
            _log.LogDebug("Using basic auth");
            return new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }

        return null;
    }

    private AuthenticationHeaderValue GetBearerTokenHeader()
    {
        var bearerTokenFromConfig = _config.GetValue<string>("Auth:BearerToken");

        if (!string.IsNullOrWhiteSpace(bearerTokenFromConfig))
        {
            _log.LogDebug("Using static bearer token");
            return new AuthenticationHeaderValue("Bearer", bearerTokenFromConfig);
        }

        return null;
    }
}
