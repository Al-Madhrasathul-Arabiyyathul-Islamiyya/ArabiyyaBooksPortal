using Microsoft.Extensions.Configuration;

namespace BooksPortal.IntegrationTests;

public sealed record IntegrationTestSettings(
    string DatabaseConnection,
    string AdminEmail,
    string AdminPassword);

public static class TestSettings
{
    private static readonly Lazy<IntegrationTestSettings> _current = new(Load);

    public static IntegrationTestSettings Current => _current.Value;

    private static IntegrationTestSettings Load()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.Test.json", optional: false, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        var dbConnection = FirstNonEmpty(
            Environment.GetEnvironmentVariable("INTEGRATION_TEST_CONNECTION_STRING"),
            configuration.GetConnectionString("IntegrationTestConnection"),
            configuration["IntegrationTest:ConnectionString"]);

        var adminEmail = FirstNonEmpty(
            Environment.GetEnvironmentVariable("INTEGRATION_TEST_ADMIN_EMAIL"),
            configuration["IntegrationTest:Admin:Email"]);

        var adminPassword = FirstNonEmpty(
            Environment.GetEnvironmentVariable("INTEGRATION_TEST_ADMIN_PASSWORD"),
            configuration["IntegrationTest:Admin:Password"]);

        return new IntegrationTestSettings(
            dbConnection ?? throw new InvalidOperationException("Integration test DB connection is missing."),
            adminEmail ?? throw new InvalidOperationException("Integration test admin email is missing."),
            adminPassword ?? throw new InvalidOperationException("Integration test admin password is missing."));
    }

    private static string? FirstNonEmpty(params string?[] values)
        => values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
}
