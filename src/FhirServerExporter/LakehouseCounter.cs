using DuckDB.NET.Data;
using Hl7.Fhir.Model;

namespace FhirServerExporter;

public class LakehouseCounter : IDisposable, IFhirResourceCounter
{
    private readonly DuckDBConnection connection;
    private readonly LakehouseConfig config;

    public LakehouseCounter(LakehouseConfig config)
    {
        this.config = config;

        connection = new DuckDBConnection("DataSource=:memory:");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE SECRET s3_credentials (
                TYPE s3,
                PROVIDER credential_chain,
                ENDPOINT $s3_endpoint,
                REGION $s3_region,
                URL_STYLE $s3_url_style,
                USE_SSL $s3_use_ssl
            )
            """;
        command.Parameters.Add(new DuckDBParameter("s3_endpoint", config.S3.Endpoint));
        command.Parameters.Add(new DuckDBParameter("s3_region", config.S3.Region));
        command.Parameters.Add(new DuckDBParameter("s3_url_style", config.S3.UrlStyle));
        command.Parameters.Add(new DuckDBParameter("s3_use_ssl", config.S3.UseSsl));
        command.ExecuteNonQuery();
    }

    public async Task<long?> GetResourceCountAsync(
        ResourceType resourceType,
        CancellationToken cancellationToken = default
    )
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT COUNT(*)
            FROM delta_scan($database_path || '/' || $resource_type || '.parquet')
            """;

        command.Parameters.Add(new DuckDBParameter("database_path", config.DatabasePath));
        command.Parameters.Add(new DuckDBParameter("resource_type", resourceType.ToString()));

        try
        {
            var count = await command.ExecuteScalarAsync(cancellationToken);
            if (count is long x)
            {
                return x;
            }
        }
        catch (DuckDBException exc)
            when (exc.Message.Contains(
                    "No files in log segment",
                    StringComparison.InvariantCultureIgnoreCase
                )
            )
        {
            return 0;
        }

        return null;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        connection?.Close();
    }
}
