using BooksPortal.Application.Common.Interfaces;
using BooksPortal.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BooksPortal.Infrastructure.Services;

public class StaffDirectoryService : IStaffDirectoryService
{
    private readonly UserManager<Staff> _userManager;

    public StaffDirectoryService(UserManager<Staff> userManager)
    {
        _userManager = userManager;
    }

    public async Task<StaffDirectoryEntry?> GetByIdAsync(int id)
    {
        var user = await _userManager.Users
            .Where(u => u.Id == id)
            .Select(u => new { u.Id, u.FullName, u.Email, u.UserName, u.Designation })
            .FirstOrDefaultAsync();

        return user == null
            ? null
            : new StaffDirectoryEntry(
                user.Id,
                ResolveDisplayName(user.FullName),
                user.Designation);
    }

    public async Task<Dictionary<int, StaffDirectoryEntry>> GetByIdsAsync(IEnumerable<int> ids)
    {
        var distinct = ids.Distinct().ToList();
        if (distinct.Count == 0)
            return new Dictionary<int, StaffDirectoryEntry>();

        var users = await _userManager.Users
            .Where(u => distinct.Contains(u.Id))
            .Select(u => new { u.Id, u.FullName, u.Email, u.UserName, u.Designation })
            .ToListAsync();

        return users.ToDictionary(
            u => u.Id,
            u => new StaffDirectoryEntry(
                u.Id,
                ResolveDisplayName(u.FullName),
                u.Designation));
    }

    private static string ResolveDisplayName(string? fullName)
        => string.IsNullOrWhiteSpace(fullName) ? string.Empty : fullName.Trim();
}
