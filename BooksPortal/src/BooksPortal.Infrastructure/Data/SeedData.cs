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

        string[] roles = [UserRole.SuperAdmin, UserRole.Admin, UserRole.User];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<int>(role));
            }
        }

        var adminUser = await userManager.FindByEmailAsync(superAdmin.Email);
        if (adminUser == null && !string.IsNullOrWhiteSpace(superAdmin.UserName))
        {
            adminUser = await userManager.FindByNameAsync(superAdmin.UserName);
        }

        if (adminUser == null)
        {
            adminUser = new Staff
            {
                UserName = superAdmin.UserName,
                Email = superAdmin.Email,
                FullName = superAdmin.FullName,
                Designation = superAdmin.Designation,
                IsActive = true
            };

            var result = await userManager.CreateAsync(adminUser, superAdmin.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create main SuperAdmin account: {errors}");
            }
        }

        // Self-heal main superadmin account at startup.
        adminUser.UserName = superAdmin.UserName;
        adminUser.Email = superAdmin.Email;
        adminUser.FullName = superAdmin.FullName;
        adminUser.Designation = superAdmin.Designation;
        adminUser.IsActive = true;

        var updateResult = await userManager.UpdateAsync(adminUser);
        if (!updateResult.Succeeded)
        {
            var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to update main SuperAdmin account: {errors}");
        }

        if (!await userManager.IsInRoleAsync(adminUser, UserRole.SuperAdmin))
        {
            var roleResult = await userManager.AddToRoleAsync(adminUser, UserRole.SuperAdmin);
            if (!roleResult.Succeeded)
            {
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to assign SuperAdmin role to main account: {errors}");
            }
        }

        var passwordValid = await userManager.CheckPasswordAsync(adminUser, superAdmin.Password);
        if (!passwordValid)
        {
            if (adminUser.PasswordHash is not null)
            {
                var removePasswordResult = await userManager.RemovePasswordAsync(adminUser);
                if (!removePasswordResult.Succeeded)
                {
                    var errors = string.Join(", ", removePasswordResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to reset main SuperAdmin password: {errors}");
                }
            }

            var addPasswordResult = await userManager.AddPasswordAsync(adminUser, superAdmin.Password);
            if (!addPasswordResult.Succeeded)
            {
                var errors = string.Join(", ", addPasswordResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to set main SuperAdmin password: {errors}");
            }
        }

        var db = serviceProvider.GetRequiredService<BooksPortalDbContext>();

        await CurriculumSeeder.SeedAsync(db);
        await ClassSectionSeeder.SeedAsync(db);
        await SubjectAndBookSeeder.SeedAsync(db);
        await PeopleSeeder.SeedAsync(db);
        await SlipTemplateSettingsSeeder.SeedAsync(db);
    }
}
