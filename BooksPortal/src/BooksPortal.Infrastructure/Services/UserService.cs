using BooksPortal.Application.Common.Exceptions;
using BooksPortal.Application.Common.Interfaces;
using BooksPortal.Application.Common.Models;
using BooksPortal.Application.Features.Users.DTOs;
using BooksPortal.Domain.Enums;
using BooksPortal.Infrastructure.Data;
using BooksPortal.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BooksPortal.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly UserManager<Staff> _userManager;
    private readonly RoleManager<IdentityRole<int>> _roleManager;
    private readonly SuperAdminAccountSettings _superAdmin;

    public UserService(
        UserManager<Staff> userManager,
        RoleManager<IdentityRole<int>> roleManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _superAdmin = configuration.GetSection("SuperAdminSeed").Get<SuperAdminAccountSettings>()
            ?? new SuperAdminAccountSettings();
    }

    public async Task<PaginatedList<UserResponse>> GetPagedUsersAsync(
        int pageNumber,
        int pageSize,
        string? search = null,
        bool? isActive = null,
        string? role = null)
    {
        IQueryable<Staff> query = _userManager.Users;

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(u =>
                (u.UserName != null && u.UserName.Contains(term)) ||
                (u.Email != null && u.Email.Contains(term)) ||
                (u.FullName != null && u.FullName.Contains(term)) ||
                (u.NationalId != null && u.NationalId.Contains(term)));
        }

        if (isActive.HasValue)
            query = query.Where(u => u.IsActive == isActive.Value);

        List<Staff> users;
        int totalCount;

        if (!string.IsNullOrWhiteSpace(role))
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(role);
            query = usersInRole.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(u =>
                    (u.UserName != null && u.UserName.Contains(term)) ||
                    (u.Email != null && u.Email.Contains(term)) ||
                    (u.FullName != null && u.FullName.Contains(term)) ||
                    (u.NationalId != null && u.NationalId.Contains(term)));
            }

            if (isActive.HasValue)
                query = query.Where(u => u.IsActive == isActive.Value);

            totalCount = query.Count();
            users = query
                .OrderBy(u => u.FullName ?? u.UserName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }
        else
        {
            totalCount = await query.CountAsync();
            users = await query
                .OrderBy(u => u.FullName ?? u.UserName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        var result = new List<UserResponse>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            result.Add(MapToResponse(user, roles));
        }

        return new PaginatedList<UserResponse>(result, totalCount, pageNumber, pageSize);
    }

    public async Task<UserResponse> GetUserByIdAsync(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString())
            ?? throw new NotFoundException("Staff", id);

        var roles = await _userManager.GetRolesAsync(user);
        return MapToResponse(user, roles);
    }

    public async Task<UserResponse> CreateUserAsync(CreateUserRequest request)
    {
        var user = new Staff
        {
            UserName = request.UserName,
            Email = request.Email,
            FullName = request.FullName,
            NationalId = request.NationalId,
            Designation = request.Designation,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new BusinessRuleException(errors);
        }

        if (request.Roles.Count > 0)
        {
            await _userManager.AddToRolesAsync(user, request.Roles);
        }

        var roles = await _userManager.GetRolesAsync(user);
        return MapToResponse(user, roles);
    }

    public async Task<UserResponse> UpdateUserAsync(int id, UpdateUserRequest request)
    {
        var user = await _userManager.FindByIdAsync(id.ToString())
            ?? throw new NotFoundException("Staff", id);

        if (await IsMainSuperAdminAsync(user) && !request.IsActive)
        {
            throw new BusinessRuleException("Main SuperAdmin account cannot be deactivated.");
        }

        user.Email = request.Email;
        user.FullName = request.FullName;
        user.NationalId = request.NationalId;
        user.Designation = request.Designation;
        user.IsActive = request.IsActive;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new BusinessRuleException(errors);
        }

        var roles = await _userManager.GetRolesAsync(user);
        return MapToResponse(user, roles);
    }

    public async Task ToggleUserActiveAsync(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString())
            ?? throw new NotFoundException("Staff", id);

        // Allow reactivation if an external action disabled it, but never allow deactivation.
        if (await IsMainSuperAdminAsync(user) && user.IsActive)
        {
            throw new BusinessRuleException("Main SuperAdmin account cannot be deactivated.");
        }

        user.IsActive = !user.IsActive;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new BusinessRuleException(errors);
        }
    }

    public async Task AssignRolesAsync(int id, List<string> roles)
    {
        var user = await _userManager.FindByIdAsync(id.ToString())
            ?? throw new NotFoundException("Staff", id);

        if (await IsMainSuperAdminAsync(user) &&
            !roles.Any(r => string.Equals(r, UserRole.SuperAdmin, StringComparison.OrdinalIgnoreCase)))
        {
            throw new BusinessRuleException("Main SuperAdmin account must retain SuperAdmin role.");
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRolesAsync(user, roles);
    }

    private async Task<bool> IsMainSuperAdminAsync(Staff user)
    {
        var emailMatches = !string.IsNullOrWhiteSpace(_superAdmin.Email) &&
            string.Equals(user.Email, _superAdmin.Email, StringComparison.OrdinalIgnoreCase);
        var userNameMatches = !string.IsNullOrWhiteSpace(_superAdmin.UserName) &&
            string.Equals(user.UserName, _superAdmin.UserName, StringComparison.OrdinalIgnoreCase);

        if (!emailMatches && !userNameMatches)
            return false;

        return await _userManager.IsInRoleAsync(user, UserRole.SuperAdmin);
    }

    private static UserResponse MapToResponse(Staff user, IList<string> roles)
    {
        return new UserResponse
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            NationalId = user.NationalId,
            Designation = user.Designation,
            IsActive = user.IsActive,
            Roles = roles.ToList(),
            CreatedAt = user.CreatedAt
        };
    }
}
