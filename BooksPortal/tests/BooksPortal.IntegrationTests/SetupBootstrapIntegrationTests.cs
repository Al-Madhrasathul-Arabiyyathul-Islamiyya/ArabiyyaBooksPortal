using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BooksPortal.Application.Features.Setup.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;

namespace BooksPortal.IntegrationTests;

[Collection("Integration API")]
public class SetupBootstrapIntegrationTests
{
    [Fact]
    public async Task Bootstrap_Status_Is_Accessible_Without_Authentication()
    {
        await using var factory = new ProductionBootstrapWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/setup/bootstrap/status");
        response.EnsureSuccessStatusCode();

        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = body.RootElement.GetProperty("data");
        data.GetProperty("issues").EnumerateArray()
            .Any(x => x.GetProperty("key").GetString() == "super-admin")
            .Should().BeTrue();
    }

    [Fact]
    public async Task Bootstrap_Creates_First_SuperAdmin_And_Blocks_Second_Attempt()
    {
        await using var factory = new ProductionBootstrapWebApplicationFactory();
        var client = factory.CreateClient();

        var loginBefore = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "admin@booksportal.local",
            password = "Admin@123456"
        });
        loginBefore.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var bootstrapRequest = new BootstrapSuperAdminRequest
        {
            UserName = "bootstrapadmin",
            Email = "bootstrap.admin@booksportal.local",
            Password = "Bootstrap123",
            FullName = "Bootstrap Admin",
            NationalId = "BOOT-0001",
            Designation = "System Administrator"
        };

        var bootstrapResponse = await client.PostAsJsonAsync("/api/setup/bootstrap/super-admin", bootstrapRequest);
        bootstrapResponse.EnsureSuccessStatusCode();

        var loginAfter = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = bootstrapRequest.Email,
            password = bootstrapRequest.Password
        });
        loginAfter.EnsureSuccessStatusCode();

        var secondAttempt = await client.PostAsJsonAsync("/api/setup/bootstrap/super-admin", bootstrapRequest);
        secondAttempt.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    private sealed class ProductionBootstrapWebApplicationFactory : IntegrationTestWebApplicationFactory
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.UseEnvironment("Production");
        }
    }
}

