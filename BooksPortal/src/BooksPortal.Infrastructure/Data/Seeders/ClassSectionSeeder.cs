using BooksPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BooksPortal.Infrastructure.Data.Seeders;

internal static class ClassSectionSeeder
{
    public static async Task SeedAsync(BooksPortalDbContext db)
    {
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

        var grades = await db.Grades
            .OrderBy(g => g.SortOrder)
            .ToListAsync();
        if (grades.Count == 0)
            return;

        var sectionLetters = new[] { "A", "B", "C", "D" };
        foreach (var grade in grades)
        {
            foreach (var section in sectionLetters)
            {
                var exists = await db.ClassSections.AnyAsync(c =>
                    c.AcademicYearId == activeAcademicYear.Id
                    && c.GradeId == grade.Id
                    && c.Section == section);

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
}
