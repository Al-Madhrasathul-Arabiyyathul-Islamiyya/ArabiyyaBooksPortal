using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace BooksPortal.IntegrationTests;

public sealed class IntegrationTestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var dbName = $"BooksPortalIntegration_{Guid.NewGuid():N}";
        var connectionString = ResolveConnectionString(TestSettings.Current.DatabaseConnection, dbName);

        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = connectionString
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
}
