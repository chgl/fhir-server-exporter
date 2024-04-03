namespace FhirServerExporter;

public record AppConfig
{
    public Uri? FhirServerUrl { get; set; }

    public string FhirServerName { get; set; } = string.Empty;

    public int MetricsPort { get; set; } = 9797;

    public int FetchIntervalSeconds { get; set; } = 30;

    public string ExcludedResources { get; set; } = string.Empty;

    public string IncludedResources { get; set; } = string.Empty;

    public ICollection<CustomMetric> Queries { get; set; } = [];

    public AuthConfig Auth { get; set; } = new AuthConfig();

    public TimeSpan FhirServerTimeout { get; set; } = TimeSpan.FromMinutes(2);
}

public record AuthConfig
{
    public OAuthConfig OAuth { get; init; } = new();

    public BasicAuthConfig Basic { get; init; } = new();

    public string? BearerToken { get; init; }
}

public record OAuthConfig
{
    public Uri? TokenUrl { get; init; }

    public string? ClientId { get; init; }

    public string? ClientSecret { get; init; }

    public string? Scope { get; init; }
}

public record BasicAuthConfig
{
    public string? Username { get; init; }

    public string? Password { get; init; }
}

public record CustomMetric
{
    public string? Name { get; init; }

    public string? Query { get; init; }

    public string? Description { get; init; }
}
