using BooksPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BooksPortal.Infrastructure.Data.Seeders;

internal static class PeopleSeeder
{
    private const int TargetStudentCount = 100;
    private const int TargetParentCount = 100;
    private const int TargetTeacherCount = 20;

    public static async Task SeedAsync(BooksPortalDbContext db)
    {
        var classSectionIds = await db.ClassSections
            .OrderBy(c => c.Id)
            .Select(c => c.Id)
            .ToListAsync();

        if (classSectionIds.Count == 0)
            return;

        await SeedStudentsAsync(db, classSectionIds);
        await SeedParentsAsync(db);
        await SeedTeachersAsync(db);
        await SeedStudentParentLinksAsync(db);
    }

    private static async Task SeedStudentsAsync(BooksPortalDbContext db, IReadOnlyList<int> classSectionIds)
    {
        var existingCount = await db.Students.CountAsync();
        if (existingCount >= TargetStudentCount)
            return;

        var existingIndexNos = await db.Students
            .IgnoreQueryFilters()
            .Select(s => s.IndexNo)
            .ToHashSetAsync(StringComparer.OrdinalIgnoreCase);

        var existingNationalIds = await db.Students
            .IgnoreQueryFilters()
            .Select(s => s.NationalId)
            .ToHashSetAsync(StringComparer.OrdinalIgnoreCase);

        var next = 1;
        while (existingCount < TargetStudentCount)
        {
            var indexNo = $"S{next:000000}";
            var nationalId = $"STU-{next:000000}";

            next++;

            if (existingIndexNos.Contains(indexNo) || existingNationalIds.Contains(nationalId))
                continue;

            var classSectionId = classSectionIds[(existingCount) % classSectionIds.Count];

            db.Students.Add(new Student
            {
                FullName = $"Integration Student {indexNo}",
                IndexNo = indexNo,
                NationalId = nationalId,
                ClassSectionId = classSectionId
            });

            existingIndexNos.Add(indexNo);
            existingNationalIds.Add(nationalId);
            existingCount++;
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedParentsAsync(BooksPortalDbContext db)
    {
        var existingCount = await db.Parents.CountAsync();
        if (existingCount >= TargetParentCount)
            return;

        var existingNationalIds = await db.Parents
            .IgnoreQueryFilters()
            .Select(p => p.NationalId)
            .ToHashSetAsync(StringComparer.OrdinalIgnoreCase);

        var next = 1;
        while (existingCount < TargetParentCount)
        {
            var nationalId = $"PAR-{next:000000}";
            next++;

            if (existingNationalIds.Contains(nationalId))
                continue;

            db.Parents.Add(new Parent
            {
                FullName = $"Integration Parent {nationalId}",
                NationalId = nationalId,
                Phone = $"7{(700000 + existingCount):000000}",
                Relationship = "Parent"
            });

            existingNationalIds.Add(nationalId);
            existingCount++;
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedTeachersAsync(BooksPortalDbContext db)
    {
        var existingCount = await db.Teachers.CountAsync();
        if (existingCount >= TargetTeacherCount)
            return;

        var existingNationalIds = await db.Teachers
            .IgnoreQueryFilters()
            .Select(t => t.NationalId)
            .ToHashSetAsync(StringComparer.OrdinalIgnoreCase);

        var next = 1;
        while (existingCount < TargetTeacherCount)
        {
            var nationalId = $"TEA-{next:000000}";
            next++;

            if (existingNationalIds.Contains(nationalId))
                continue;

            db.Teachers.Add(new Teacher
            {
                FullName = $"Integration Teacher {nationalId}",
                NationalId = nationalId,
                Email = $"teacher.{nationalId.ToLowerInvariant()}@booksportal.local",
                Phone = $"9{(200000 + existingCount):000000}"
            });

            existingNationalIds.Add(nationalId);
            existingCount++;
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedStudentParentLinksAsync(BooksPortalDbContext db)
    {
        var students = await db.Students
            .OrderBy(s => s.Id)
            .Select(s => s.Id)
            .ToListAsync();

        var parents = await db.Parents
            .OrderBy(p => p.Id)
            .Select(p => p.Id)
            .ToListAsync();

        if (students.Count == 0 || parents.Count == 0)
            return;

        var existingLinks = await db.StudentParents
            .IgnoreQueryFilters()
            .Select(sp => new { sp.StudentId, sp.ParentId })
            .ToListAsync();

        var existingMap = existingLinks
            .GroupBy(l => l.StudentId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.ParentId).ToHashSet());

        var added = false;
        for (var i = 0; i < students.Count; i++)
        {
            var studentId = students[i];
            if (existingMap.ContainsKey(studentId) && existingMap[studentId].Count > 0)
                continue;

            var parentId = parents[i % parents.Count];
            db.StudentParents.Add(new StudentParent
            {
                StudentId = studentId,
                ParentId = parentId,
                IsPrimary = true
            });
            added = true;
        }

        if (added)
            await db.SaveChangesAsync();
    }
}
