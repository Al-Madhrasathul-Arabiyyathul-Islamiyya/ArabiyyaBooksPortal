using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BooksPortal.IntegrationTests;

[Collection("Integration API")]
public class HealthCheckTests : IClassFixture<IntegrationTestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthCheckTests(IntegrationTestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsOk()
    {
        var response = await _client.GetAsync("/health");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }
}
