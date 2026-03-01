using BooksPortal.Application.Features.Settings.Services;
using Microsoft.EntityFrameworkCore;

namespace BooksPortal.Infrastructure.Data.Seeders;

internal static class SlipTemplateSettingsSeeder
{
    public static async Task SeedAsync(BooksPortalDbContext db)
    {
        var defaults = SlipTemplateSettingService.GetDefaultLabels();
        var existing = await db.SlipTemplateSettings
            .ToListAsync();

        if (existing.Count == 0)
        {
            db.SlipTemplateSettings.AddRange(defaults);
            await db.SaveChangesAsync();
            return;
        }

        var existingByKey = existing
            .ToDictionary(x => $"{x.Category}::{x.Key}", StringComparer.OrdinalIgnoreCase);

        var changed = false;
        foreach (var item in defaults)
        {
            var composite = $"{item.Category}::{item.Key}";
            if (!existingByKey.TryGetValue(composite, out var current))
            {
                db.SlipTemplateSettings.Add(item);
                changed = true;
                continue;
            }

            if (!string.Equals(current.Value, item.Value, StringComparison.Ordinal)
                || current.SortOrder != item.SortOrder)
            {
                current.Value = item.Value;
                current.SortOrder = item.SortOrder;
                changed = true;
            }
        }

        if (changed)
            await db.SaveChangesAsync();
    }
}
