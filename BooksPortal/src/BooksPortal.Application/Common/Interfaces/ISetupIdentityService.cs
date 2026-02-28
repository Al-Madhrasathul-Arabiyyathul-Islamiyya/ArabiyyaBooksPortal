namespace BooksPortal.Application.Common.Interfaces;

public interface ISetupIdentityService
{
    Task<bool> HasActiveSuperAdminAsync(CancellationToken cancellationToken = default);
}
