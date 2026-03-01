using BooksPortal.Domain.Entities;
using BooksPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BooksPortal.Infrastructure.Data.Seeders;

internal static class ReferenceNumberFormatSeeder
{
    private static readonly (SlipType SlipType, string Template)[] Defaults =
    [
        (SlipType.Distribution, "DST{year}{autonum}"),
        (SlipType.Return, "RTN{year}{autonum}"),
        (SlipType.TeacherIssue, "TIS{year}{autonum}"),
        (SlipType.TeacherReturn, "TRT{year}{autonum}")
    ];

    public static async Task SeedAsync(BooksPortalDbContext db)
    {
        var academicYearIds = await db.AcademicYears
            .AsNoTracking()
            .OrderBy(a => a.Id)
            .Select(a => a.Id)
            .ToListAsync();

        if (academicYearIds.Count == 0)
            return;

        var existing = await db.ReferenceNumberFormats
            .IgnoreQueryFilters()
            .ToListAsync();

        var changed = false;
        foreach (var yearId in academicYearIds)
        {
            foreach (var (slipType, template) in Defaults)
            {
                var current = existing.FirstOrDefault(x =>
                    x.AcademicYearId == yearId && x.SlipType == slipType);

                if (current == null)
                {
                    db.ReferenceNumberFormats.Add(new ReferenceNumberFormat
                    {
                        AcademicYearId = yearId,
                        SlipType = slipType,
                        FormatTemplate = template,
                        PaddingWidth = 6
                    });
                    changed = true;
                    continue;
                }

                if (current.IsDeleted)
                {
                    current.IsDeleted = false;
                    current.DeletedAt = null;
                    changed = true;
                }

                if (!string.Equals(current.FormatTemplate, template, StringComparison.Ordinal)
                    || current.PaddingWidth != 6)
                {
                    current.FormatTemplate = template;
                    current.PaddingWidth = 6;
                    changed = true;
                }
            }
        }

        if (changed)
            await db.SaveChangesAsync();
    }
}
