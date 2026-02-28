using BooksPortal.Application.Features.Setup.DTOs;

namespace BooksPortal.Application.Features.Setup.Interfaces;

public interface ISetupReadinessService
{
    Task<SetupStatusResponse> GetStatusAsync(CancellationToken cancellationToken = default);
    Task<SetupStatusResponse> StartAsync(CancellationToken cancellationToken = default);
    Task<SetupStatusResponse> ConfirmSuperAdminAsync(CancellationToken cancellationToken = default);
    Task<SetupStatusResponse> ConfirmSlipTemplatesAsync(CancellationToken cancellationToken = default);
    Task<SetupStatusResponse> InitializeHierarchyAsync(CancellationToken cancellationToken = default);
    Task<SetupStatusResponse> InitializeReferenceFormatsAsync(CancellationToken cancellationToken = default);
    Task<SetupStatusResponse> CompleteAsync(CancellationToken cancellationToken = default);
    Task<SetupStatusResponse> EnsureBackfillStateAsync(CancellationToken cancellationToken = default);
    Task EnsureReadyOrThrowAsync(CancellationToken cancellationToken = default);
}
