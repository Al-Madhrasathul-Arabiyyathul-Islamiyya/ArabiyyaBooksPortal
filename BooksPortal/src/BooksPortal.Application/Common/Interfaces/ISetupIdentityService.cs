namespace BooksPortal.Application.Common.Interfaces;

public interface ISetupIdentityService
{
    Task<bool> HasActiveSuperAdminAsync(CancellationToken cancellationToken = default);
    Task<bool> HasAnySuperAdminAsync(CancellationToken cancellationToken = default);
    Task CreateSuperAdminAsync(
        string userName,
        string email,
        string password,
        string fullName,
        string? nationalId,
        string? designation,
        CancellationToken cancellationToken = default);
}
