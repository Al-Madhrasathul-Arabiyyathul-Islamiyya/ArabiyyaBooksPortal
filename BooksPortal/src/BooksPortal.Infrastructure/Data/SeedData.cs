using BooksPortal.Application.Features.Settings.Services;
using BooksPortal.Domain.Enums;
using BooksPortal.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BooksPortal.Infrastructure.Data;

public static class SeedData
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<Staff>>();

        string[] roles = [UserRole.SuperAdmin, UserRole.Admin, UserRole.User];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<int>(role));
            }
        }

        var adminEmail = "admin@booksportal.local";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new Staff
            {
                UserName = "admin",
                Email = adminEmail,
                FullName = "System Administrator",
                IsActive = true
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123456");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, UserRole.SuperAdmin);
            }
        }

        await SeedSlipTemplateSettingsAsync(serviceProvider);
    }

    private static async Task SeedSlipTemplateSettingsAsync(IServiceProvider serviceProvider)
    {
        var db = serviceProvider.GetRequiredService<BooksPortalDbContext>();

        if (await db.SlipTemplateSettings.AnyAsync())
            return;

        var defaults = SlipTemplateSettingService.GetDefaultLabels();
        db.SlipTemplateSettings.AddRange(defaults);
        await db.SaveChangesAsync();
    }
}
