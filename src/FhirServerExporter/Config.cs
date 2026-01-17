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

    public LakehouseConfig FhirLakehouse { get; set; } = new LakehouseConfig();
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

public record LakehouseConfig
{
    public string? DatabasePath { get; init; }

    public S3Config S3 { get; init; } = new();
}

public record S3Config
{
    public string? Endpoint { get; init; }

    public string? Region { get; init; }

    public string? UrlStyle { get; init; }

    public bool? UseSsl { get; init; }
}
