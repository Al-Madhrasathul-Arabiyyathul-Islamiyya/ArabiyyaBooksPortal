using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BooksPortal.Application.Common.Exceptions;
using BooksPortal.Application.Features.Setup.DTOs;
using BooksPortal.Application.Features.Setup.Interfaces;
using BooksPortal.Domain.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BooksPortal.IntegrationTests;

[Collection("Integration API")]
public class SetupReadinessIntegrationTests : IClassFixture<IntegrationTestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SetupReadinessIntegrationTests(IntegrationTestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Setup_Status_Returns_StepChecklist()
    {
        await AuthenticateAsync(TestSettings.Current.AdminEmail, TestSettings.Current.AdminPassword);

        var response = await _client.GetAsync("/api/setup/status");
        response.EnsureSuccessStatusCode();

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = json.RootElement.GetProperty("data");
        data.GetProperty("steps").EnumerateArray().Any().Should().BeTrue();
    }

    [Fact]
    public async Task Guarded_Write_Endpoint_Returns_SetupIncomplete_When_NotReady()
    {
        await using var factory = new IncompleteSetupWebApplicationFactory();
        var client = factory.CreateClient();
        await AuthenticateAsync(client, TestSettings.Current.AdminEmail, TestSettings.Current.AdminPassword);

        var response = await client.PostAsJsonAsync("/api/distributions", new { });
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("SETUP_INCOMPLETE");
        body.Should().Contain("super-admin");
    }

    private async Task AuthenticateAsync(string email, string password)
        => await AuthenticateAsync(_client, email, password);

    private static async Task AuthenticateAsync(HttpClient client, string email, string password)
    {
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password
        });
        loginResponse.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await loginResponse.Content.ReadAsStringAsync());
        var token = doc.RootElement.GetProperty("data").GetProperty("accessToken").GetString();
        token.Should().NotBeNullOrWhiteSpace();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private sealed class IncompleteSetupWebApplicationFactory : IntegrationTestWebApplicationFactory
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ISetupReadinessService>();
                services.AddScoped<ISetupReadinessService, AlwaysIncompleteSetupReadinessService>();
            });
        }
    }

    private sealed class AlwaysIncompleteSetupReadinessService : ISetupReadinessService
    {
        public Task<SetupStatusResponse> GetStatusAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(BuildIncomplete());

        public Task<SetupStatusResponse> GetBootstrapStatusAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(BuildIncomplete());

        public Task<SetupStatusResponse> BootstrapSuperAdminAsync(
            BootstrapSuperAdminRequest request,
            CancellationToken cancellationToken = default)
            => Task.FromResult(BuildIncomplete());

        public Task<SetupStatusResponse> StartAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(BuildIncomplete());

        public Task<SetupStatusResponse> ConfirmSuperAdminAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(BuildIncomplete());

        public Task<SetupStatusResponse> ConfirmSlipTemplatesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(BuildIncomplete());

        public Task<SetupStatusResponse> InitializeHierarchyAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(BuildIncomplete());

        public Task<SetupStatusResponse> InitializeReferenceFormatsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(BuildIncomplete());

        public Task<SetupStatusResponse> CompleteAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(BuildIncomplete());

        public Task<SetupStatusResponse> EnsureBackfillStateAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(BuildIncomplete());

        public Task EnsureReadyOrThrowAsync(CancellationToken cancellationToken = default)
            => throw new SetupIncompleteException(
                "System setup is incomplete.",
                [SetupReadinessStepKeys.SuperAdmin],
                ["Create and activate a SuperAdmin account."]);

        private static SetupStatusResponse BuildIncomplete()
            => new()
            {
                Status = SetupStatus.InProgress,
                IsReady = false,
                Steps =
                [
                    new SetupStepStatusResponse
                    {
                        Key = SetupReadinessStepKeys.SuperAdmin,
                        Title = "SuperAdmin Account",
                        IsComplete = false
                    }
                ],
                Issues =
                [
                    new SetupReadinessIssueResponse
                    {
                        Key = SetupReadinessStepKeys.SuperAdmin,
                        Message = "SuperAdmin Account is incomplete."
                    }
                ]
            };
    }
}
