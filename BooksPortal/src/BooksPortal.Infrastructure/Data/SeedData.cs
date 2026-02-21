using BooksPortal.Application.Features.Settings.Services;
using BooksPortal.Domain.Entities;
using BooksPortal.Domain.Enums;
using BooksPortal.Infrastructure.Data.Seeders;
using BooksPortal.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BooksPortal.Infrastructure.Data;

public static class SeedData
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<Staff>>();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var superAdmin = configuration.GetSection("SuperAdminSeed").Get<SuperAdminAccountSettings>()
            ?? new SuperAdminAccountSettings();
        var admin = configuration.GetSection("AdminSeed").Get<SeedAccountSettings>()
            ?? new SeedAccountSettings
            {
                UserName = "opsadmin",
                Email = "opsadmin@booksportal.local",
                Password = "Admin@123456",
                FullName = "Operations Administrator",
                NationalId = "A0000002",
                Designation = "Operations Administrator"
            };
        var user = configuration.GetSection("UserSeed").Get<SeedAccountSettings>()
            ?? new SeedAccountSettings
            {
                UserName = "operationsuser",
                Email = "user@booksportal.local",
                Password = "Admin@123456",
                FullName = "Operations User",
                NationalId = "A0000003",
                Designation = "Operations User"
            };

        string[] roles = [UserRole.SuperAdmin, UserRole.Admin, UserRole.User];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<int>(role));
            }
        }

        await EnsureSeededAccountAsync(userManager, superAdmin, UserRole.SuperAdmin, "SuperAdmin");
        await EnsureSeededAccountAsync(userManager, admin, UserRole.Admin, "Admin");
        await EnsureSeededAccountAsync(userManager, user, UserRole.User, "User");

        var db = serviceProvider.GetRequiredService<BooksPortalDbContext>();

        await CurriculumSeeder.SeedAsync(db);
        await ReferenceNumberFormatSeeder.SeedAsync(db);
        await ClassSectionSeeder.SeedAsync(db);
        await SubjectAndBookSeeder.SeedAsync(db);
        await PeopleSeeder.SeedAsync(db);
        await SlipTemplateSettingsSeeder.SeedAsync(db);
    }

    private static async Task EnsureSeededAccountAsync(
        UserManager<Staff> userManager,
        SeedAccountSettings settings,
        string role,
        string displayName)
    {
        if (string.IsNullOrWhiteSpace(settings.Email) || string.IsNullOrWhiteSpace(settings.UserName))
        {
            throw new InvalidOperationException($"{displayName} seed account must include both email and username.");
        }

        var account = await userManager.FindByEmailAsync(settings.Email);
        if (account == null)
        {
            account = await userManager.FindByNameAsync(settings.UserName);
        }

        if (account == null)
        {
            account = new Staff
            {
                UserName = settings.UserName,
                Email = settings.Email,
                FullName = settings.FullName,
                NationalId = settings.NationalId,
                Designation = settings.Designation,
                IsActive = true
            };

            var createResult = await userManager.CreateAsync(account, settings.Password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create {displayName} seed account: {errors}");
            }
        }

        account.UserName = settings.UserName;
        account.Email = settings.Email;
        account.FullName = settings.FullName;
        account.NationalId = settings.NationalId;
        account.Designation = settings.Designation;
        account.IsActive = true;

        var updateResult = await userManager.UpdateAsync(account);
        if (!updateResult.Succeeded)
        {
            var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to update {displayName} seed account: {errors}");
        }

        var currentRoles = await userManager.GetRolesAsync(account);
        var rolesToRemove = currentRoles.Where(existing => !string.Equals(existing, role, StringComparison.OrdinalIgnoreCase)).ToList();
        if (rolesToRemove.Count > 0)
        {
            var removeRolesResult = await userManager.RemoveFromRolesAsync(account, rolesToRemove);
            if (!removeRolesResult.Succeeded)
            {
                var errors = string.Join(", ", removeRolesResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to remove old roles from {displayName} seed account: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(account, role))
        {
            var addRoleResult = await userManager.AddToRoleAsync(account, role);
            if (!addRoleResult.Succeeded)
            {
                var errors = string.Join(", ", addRoleResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to assign {role} role to {displayName} seed account: {errors}");
            }
        }

        var passwordValid = await userManager.CheckPasswordAsync(account, settings.Password);
        if (!passwordValid)
        {
            if (account.PasswordHash is not null)
            {
                var removePasswordResult = await userManager.RemovePasswordAsync(account);
                if (!removePasswordResult.Succeeded)
                {
                    var errors = string.Join(", ", removePasswordResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to reset {displayName} seed password: {errors}");
                }
            }

            var addPasswordResult = await userManager.AddPasswordAsync(account, settings.Password);
            if (!addPasswordResult.Succeeded)
            {
                var errors = string.Join(", ", addPasswordResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to set {displayName} seed password: {errors}");
            }
        }
    }
}
