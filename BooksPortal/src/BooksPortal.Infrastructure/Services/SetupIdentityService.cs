using BooksPortal.Application.Common.Interfaces;
using BooksPortal.Domain.Enums;
using BooksPortal.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace BooksPortal.Infrastructure.Services;

public class SetupIdentityService : ISetupIdentityService
{
    private readonly UserManager<Staff> _userManager;

    public SetupIdentityService(UserManager<Staff> userManager)
    {
        _userManager = userManager;
    }

    public async Task<bool> HasActiveSuperAdminAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userManager.GetUsersInRoleAsync(UserRole.SuperAdmin);
        return users.Any(x => x.IsActive);
    }
}
