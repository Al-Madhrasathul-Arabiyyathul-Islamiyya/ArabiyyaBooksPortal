using BooksPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BooksPortal.Infrastructure.Data.Seeders;

internal static class CurriculumSeeder
{
    public static async Task SeedAsync(BooksPortalDbContext db)
    {
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
}
