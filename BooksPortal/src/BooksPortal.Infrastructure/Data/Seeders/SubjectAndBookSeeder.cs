using BooksPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BooksPortal.Infrastructure.Data.Seeders;

internal static class SubjectAndBookSeeder
{
    private sealed record SubjectSeedItem(string Code, string Name);
    private sealed record BookTemplate(string SubjectCode, string SubjectName, string TitlePattern);

    public static async Task SeedAsync(BooksPortalDbContext db)
    {
        // Subject set based on the 2026 requisition sheet categories.
        var subjectSeeds = new[]
        {
            new SubjectSeedItem("QURAN", "Quran"),
            new SubjectSeedItem("DHIVEHI", "Dhivehi"),
            new SubjectSeedItem("ENGLISH", "English"),
            new SubjectSeedItem("SCIENCE", "Science"),
            new SubjectSeedItem("SOCIAL", "Social Studies"),
            new SubjectSeedItem("CA", "Creative Arts"),
            new SubjectSeedItem("HPE", "Health & Physical Education"),
        };

        foreach (var seed in subjectSeeds)
        {
            var subject = await db.Subjects.FirstOrDefaultAsync(s => s.Code == seed.Code);
            if (subject == null)
            {
                db.Subjects.Add(new Subject
                {
                    Code = seed.Code,
                    Name = seed.Name
                });
            }
            else if (!string.Equals(subject.Name, seed.Name, StringComparison.Ordinal))
            {
                subject.Name = seed.Name;
                db.Subjects.Update(subject);
            }
        }

        await db.SaveChangesAsync();

        var subjectsByCode = await db.Subjects
            .ToDictionaryAsync(s => s.Code, s => s);

        // Keep the generated catalog deterministic and idempotent.
        var templates = new[]
        {
            new BookTemplate("QURAN", "Quran", "Thadhureesul Quran"),
            new BookTemplate("DHIVEHI", "Dhivehi", "Dhivehi"),
            new BookTemplate("ENGLISH", "English", "Exploring English"),
            new BookTemplate("SCIENCE", "Science", "Science"),
            new BookTemplate("SOCIAL", "Social Studies", "Social Studies"),
            new BookTemplate("CA", "Creative Arts", "Creative Arts"),
            new BookTemplate("HPE", "Health & Physical Education", "Health and Physical Education"),
        };

        var existingCodes = await db.Books
            .Select(b => b.Code)
            .ToHashSetAsync();

        var random = new Random(2026);
        foreach (var grade in Enumerable.Range(1, 12))
        {
            foreach (var template in templates)
            {
                if (!subjectsByCode.TryGetValue(template.SubjectCode, out var subject))
                    continue;

                var code = $"{template.SubjectCode[..Math.Min(template.SubjectCode.Length, 4)]}-G{grade:00}";
                if (existingCodes.Contains(code))
                    continue;

                var stock = random.Next(35, 91);
                db.Books.Add(new Book
                {
                    Code = code,
                    Title = $"{template.TitlePattern} {grade}",
                    SubjectId = subject.Id,
                    Grade = $"Grade {grade}",
                    Publisher = "Other",
                    PublishedYear = 2026,
                    ISBN = null,
                    Author = null,
                    Edition = null,
                    TotalStock = stock,
                    Distributed = 0,
                    WithTeachers = 0,
                    Damaged = 0,
                    Lost = 0
                });
                existingCodes.Add(code);
            }
        }

        await db.SaveChangesAsync();
    }
}
