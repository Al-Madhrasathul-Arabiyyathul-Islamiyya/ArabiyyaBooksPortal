using FluentAssertions;

namespace BooksPortal.IntegrationTests;

[Collection("Integration API")]
public class AccessLoggingIntegrationTests : IClassFixture<IntegrationTestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AccessLoggingIntegrationTests(IntegrationTestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthRequest_WritesStructuredAccessLogEvent()
    {
        var logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
        Directory.CreateDirectory(logDirectory);

        var snapshot = Directory.GetFiles(logDirectory, "log-*.txt")
            .ToDictionary(path => path, path => new FileInfo(path).Length);

        var response = await _client.GetAsync("/health");
        response.EnsureSuccessStatusCode();

        var found = await WaitForLogMatchAsync(
            logDirectory,
            snapshot,
            line => line.Contains("\"RequestPath\":\"/health\"")
                && line.Contains("\"RequestMethod\":\"GET\"")
                && line.Contains("\"StatusCode\":200"),
            timeout: TimeSpan.FromSeconds(10));

        found.Should().BeTrue("a structured Serilog request log event should be emitted for /health");
    }

    private static async Task<bool> WaitForLogMatchAsync(
        string logDirectory,
        IReadOnlyDictionary<string, long> snapshot,
        Func<string, bool> predicate,
        TimeSpan timeout)
    {
        var started = DateTime.UtcNow;

        while (DateTime.UtcNow - started < timeout)
        {
            var files = Directory.GetFiles(logDirectory, "log-*.txt");
            foreach (var file in files)
            {
                var initialLength = snapshot.TryGetValue(file, out var length) ? length : 0;
                var info = new FileInfo(file);
                if (info.Length <= initialLength)
                    continue;

                using var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                stream.Seek(initialLength, SeekOrigin.Begin);
                using var reader = new StreamReader(stream);

                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    if (line is not null && predicate(line))
                        return true;
                }
            }

            await Task.Delay(250);
        }

        return false;
    }
}

