using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Threading;

namespace BooksPortal.IntegrationTests;

public class IntegrationTestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"BooksPortalIntegration_{Guid.NewGuid():N}";
    private readonly string _connectionString;
    private readonly string _templatePath;
    private int _cleanupAttempted;

    public IntegrationTestWebApplicationFactory()
    {
        _connectionString = ResolveConnectionString(TestSettings.Current.DatabaseConnection, _dbName);
        _templatePath = Path.Combine(Path.GetTempPath(), "BooksPortal", "import-templates", _dbName);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _connectionString,
                ["ImportTemplateCache:StoragePath"] = _templatePath
            });
        });
    }

    private static string ResolveConnectionString(string baseConnection, string databaseName)
    {
        var builder = new SqlConnectionStringBuilder(baseConnection)
        {
            InitialCatalog = databaseName,
            TrustServerCertificate = true
        };
        return builder.ConnectionString;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            CleanupDatabaseAsync().GetAwaiter().GetResult();
        }
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await CleanupDatabaseAsync();
    }

    private async Task CleanupDatabaseAsync()
    {
        if (Interlocked.Exchange(ref _cleanupAttempted, 1) == 1)
            return;

        var builder = new SqlConnectionStringBuilder(_connectionString)
        {
            InitialCatalog = "master"
        };

        await using var connection = new SqlConnection(builder.ConnectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandType = CommandType.Text;
        command.CommandText = """
            IF DB_ID(@dbName) IS NOT NULL
            BEGIN
                DECLARE @sql nvarchar(max) =
                    N'ALTER DATABASE [' + @dbName + N'] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;'
                  + N'DROP DATABASE [' + @dbName + N'];';
                EXEC sp_executesql @sql;
            END
            """;
        command.Parameters.Add(new SqlParameter("@dbName", SqlDbType.NVarChar, 128) { Value = _dbName });

        try
        {
            await command.ExecuteNonQueryAsync();
        }
        catch
        {
            // Test cleanup must not mask test failures.
        }

        try
        {
            if (Directory.Exists(_templatePath))
            {
                Directory.Delete(_templatePath, recursive: true);
            }
        }
        catch
        {
            // Test cleanup must not mask test failures.
        }
    }
}
