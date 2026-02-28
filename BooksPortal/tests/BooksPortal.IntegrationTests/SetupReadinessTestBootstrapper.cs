using BooksPortal.Application.Features.Setup.Interfaces;
using BooksPortal.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace BooksPortal.IntegrationTests;

internal static class SetupReadinessTestBootstrapper
{
    public static async Task EnsureReadyAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var readinessService = scope.ServiceProvider.GetRequiredService<ISetupReadinessService>();

        await readinessService.EnsureBackfillStateAsync();

        var status = await readinessService.GetStatusAsync();
        if (status.Status == SetupStatus.Completed)
        {
            return;
        }

        await readinessService.StartAsync();
        await readinessService.ConfirmSuperAdminAsync();
        await readinessService.ConfirmSlipTemplatesAsync();
        await readinessService.InitializeHierarchyAsync();
        await readinessService.InitializeReferenceFormatsAsync();
        await readinessService.CompleteAsync();
    }
}
