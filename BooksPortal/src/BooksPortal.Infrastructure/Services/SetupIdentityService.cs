using BooksPortal.Application.Common.Interfaces;
using BooksPortal.Domain.Enums;
using BooksPortal.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

    public async Task<bool> HasAnySuperAdminAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userManager.GetUsersInRoleAsync(UserRole.SuperAdmin);
        return users.Count > 0;
    }

    public async Task CreateSuperAdminAsync(
        string userName,
        string email,
        string password,
        string fullName,
        string? nationalId,
        string? designation,
        CancellationToken cancellationToken = default)
    {
        if (await HasAnySuperAdminAsync(cancellationToken))
            throw new InvalidOperationException("A SuperAdmin account already exists.");

        if (!string.IsNullOrWhiteSpace(nationalId))
        {
            var nationalIdExists = await _userManager.Users
                .AnyAsync(x => x.NationalId == nationalId, cancellationToken);
            if (nationalIdExists)
                throw new InvalidOperationException($"National ID '{nationalId}' is already used by another account.");
        }

        var user = new Staff
        {
            UserName = userName,
            Email = email,
            FullName = fullName,
            NationalId = nationalId,
            Designation = designation ?? string.Empty,
            IsActive = true
        };

        var createResult = await _userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create SuperAdmin account: {errors}");
        }

        var roleResult = await _userManager.AddToRoleAsync(user, UserRole.SuperAdmin);
        if (!roleResult.Succeeded)
        {
            var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to assign SuperAdmin role: {errors}");
        }
    }
}
