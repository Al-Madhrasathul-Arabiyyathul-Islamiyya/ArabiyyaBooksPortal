using BooksPortal.Application.Features.Settings.Services;
using BooksPortal.Domain.Entities;
using BooksPortal.Domain.Enums;
using BooksPortal.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

        await SeedKeystagesAndGradesAsync(serviceProvider);
        await SeedDefaultClassSectionsAsync(serviceProvider);
        await SeedSlipTemplateSettingsAsync(serviceProvider);
    }

    private static async Task SeedKeystagesAndGradesAsync(IServiceProvider serviceProvider)
    {
        var db = serviceProvider.GetRequiredService<BooksPortalDbContext>();

        var keyStageDefinitions = new[]
        {
            new
            {
                Code = "KS1",
                Name = "Key stage 1",
                SortOrder = 1,
                Grades = new[] { ("G1", "Grade 1", 1), ("G2", "Grade 2", 2), ("G3", "Grade 3", 3) }
            },
            new
            {
                Code = "KS2",
                Name = "Key stage 2",
                SortOrder = 2,
                Grades = new[] { ("G4", "Grade 4", 4), ("G5", "Grade 5", 5), ("G6", "Grade 6", 6) }
            },
            new
            {
                Code = "KS3",
                Name = "Key stage 3",
                SortOrder = 3,
                Grades = new[] { ("G7", "Grade 7", 7), ("G8", "Grade 8", 8) }
            },
            new
            {
                Code = "KS4",
                Name = "Key stage 4",
                SortOrder = 4,
                Grades = new[] { ("G9", "Grade 9", 9), ("G10", "Grade 10", 10) }
            },
            new
            {
                Code = "KS5",
                Name = "Key stage 5",
                SortOrder = 5,
                Grades = new[] { ("G11", "Grade 11", 11), ("G12", "Grade 12", 12) }
            }
        };

        foreach (var def in keyStageDefinitions)
        {
            var keystage = await db.Keystages.FirstOrDefaultAsync(k => k.Code == def.Code);
            if (keystage == null)
            {
                keystage = new Keystage
                {
                    Code = def.Code,
                    Name = def.Name,
                    SortOrder = def.SortOrder
                };
                db.Keystages.Add(keystage);
                await db.SaveChangesAsync();
            }
            else
            {
                keystage.Name = def.Name;
                keystage.SortOrder = def.SortOrder;
                db.Keystages.Update(keystage);
                await db.SaveChangesAsync();
            }

            foreach (var (gradeCode, gradeName, sortOrder) in def.Grades)
            {
                var grade = await db.Grades.FirstOrDefaultAsync(g => g.KeystageId == keystage.Id && g.Code == gradeCode);
                if (grade == null)
                {
                    db.Grades.Add(new Grade
                    {
                        KeystageId = keystage.Id,
                        Code = gradeCode,
                        Name = gradeName,
                        SortOrder = sortOrder
                    });
                }
                else
                {
                    grade.Name = gradeName;
                    grade.SortOrder = sortOrder;
                    db.Grades.Update(grade);
                }
            }

            await db.SaveChangesAsync();
        }
    }

    private static async Task SeedDefaultClassSectionsAsync(IServiceProvider serviceProvider)
    {
        var db = serviceProvider.GetRequiredService<BooksPortalDbContext>();

        var activeAcademicYear = await db.AcademicYears.FirstOrDefaultAsync(a => a.IsActive);
        if (activeAcademicYear == null)
        {
            var now = DateTime.UtcNow;
            activeAcademicYear = new AcademicYear
            {
                Name = $"AY {now.Year}",
                Year = now.Year,
                StartDate = new DateTime(now.Year, 1, 1),
                EndDate = new DateTime(now.Year, 12, 31),
                IsActive = true
            };

            db.AcademicYears.Add(activeAcademicYear);
            await db.SaveChangesAsync();
        }

        var grades = await db.Grades.OrderBy(g => g.SortOrder).ToListAsync();
        if (grades.Count == 0)
            return;

        var sectionLetters = new[] { "A", "B", "C", "D" };

        foreach (var grade in grades)
        {
            foreach (var section in sectionLetters)
            {
                var exists = await db.ClassSections.AnyAsync(c =>
                    c.AcademicYearId == activeAcademicYear.Id &&
                    c.GradeId == grade.Id &&
                    c.Section == section);

                if (!exists)
                {
                    db.ClassSections.Add(new ClassSection
                    {
                        AcademicYearId = activeAcademicYear.Id,
                        KeystageId = grade.KeystageId,
                        GradeId = grade.Id,
                        Section = section
                    });
                }
            }
        }

        await db.SaveChangesAsync();
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
