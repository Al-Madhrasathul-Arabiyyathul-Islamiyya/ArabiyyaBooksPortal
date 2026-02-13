using BooksPortal.Application.Common.Interfaces;
using BooksPortal.Application.Features.MasterData.DTOs;
using BooksPortal.Application.Features.MasterData.Interfaces;
using BooksPortal.Domain.Entities;
using BooksPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BooksPortal.Application.Features.MasterData.Services;

public class LookupService : ILookupService
{
    private readonly IRepository<AcademicYear> _academicYearRepo;
    private readonly IRepository<Keystage> _keystageRepo;
    private readonly IRepository<Grade> _gradeRepo;
    private readonly IRepository<Subject> _subjectRepo;
    private readonly IRepository<ClassSection> _classSectionRepo;
    private readonly IRepository<Student> _studentRepo;
    private readonly IRepository<Parent> _parentRepo;
    private readonly IRepository<Book> _bookRepo;
    private readonly IRepository<Teacher> _teacherRepo;
    private readonly IRepository<TeacherIssue> _teacherIssueRepo;

    public LookupService(
        IRepository<AcademicYear> academicYearRepo,
        IRepository<Keystage> keystageRepo,
        IRepository<Grade> gradeRepo,
        IRepository<Subject> subjectRepo,
        IRepository<ClassSection> classSectionRepo,
        IRepository<Student> studentRepo,
        IRepository<Parent> parentRepo,
        IRepository<Book> bookRepo,
        IRepository<Teacher> teacherRepo,
        IRepository<TeacherIssue> teacherIssueRepo)
    {
        _academicYearRepo = academicYearRepo;
        _keystageRepo = keystageRepo;
        _gradeRepo = gradeRepo;
        _subjectRepo = subjectRepo;
        _classSectionRepo = classSectionRepo;
        _studentRepo = studentRepo;
        _parentRepo = parentRepo;
        _bookRepo = bookRepo;
        _teacherRepo = teacherRepo;
        _teacherIssueRepo = teacherIssueRepo;
    }

    public async Task<List<LookupResponse>> GetAcademicYearsAsync()
        => await _academicYearRepo.Query()
            .OrderByDescending(a => a.Year)
            .Select(a => new LookupResponse { Id = a.Id, Name = a.Name })
            .ToListAsync();

    public async Task<List<LookupResponse>> GetKeystagesAsync()
        => await _keystageRepo.Query()
            .OrderBy(k => k.SortOrder)
            .Select(k => new LookupResponse { Id = k.Id, Name = k.Name })
            .ToListAsync();

    public async Task<List<LookupResponse>> GetGradesAsync(int? keystageId = null)
    {
        var query = _gradeRepo.Query().Include(g => g.Keystage).AsQueryable();
        if (keystageId.HasValue)
            query = query.Where(g => g.KeystageId == keystageId.Value);

        return await query
            .OrderBy(g => g.Keystage.SortOrder).ThenBy(g => g.SortOrder)
            .Select(g => new LookupResponse { Id = g.Id, Name = g.Name })
            .ToListAsync();
    }

    public async Task<List<LookupResponse>> GetSubjectsAsync()
        => await _subjectRepo.Query()
            .OrderBy(s => s.Name)
            .Select(s => new LookupResponse { Id = s.Id, Name = s.Name })
            .ToListAsync();

    public async Task<List<LookupResponse>> GetClassSectionsAsync(int? academicYearId = null)
    {
        IQueryable<ClassSection> query = _classSectionRepo.Query().Include(c => c.Grade);
        if (academicYearId.HasValue)
            query = query.Where(c => c.AcademicYearId == academicYearId.Value);

        return await query
            .OrderBy(c => c.Grade.SortOrder).ThenBy(c => c.Section)
            .Select(c => new LookupResponse { Id = c.Id, Name = c.Grade.Name + " " + c.Section })
            .ToListAsync();
    }

    public Task<List<LookupResponse>> GetTermsAsync()
        => Task.FromResult(Enum.GetValues<Term>()
            .Select(t => new LookupResponse { Id = (int)t, Name = t.ToString() })
            .ToList());

    public Task<List<LookupResponse>> GetBookConditionsAsync()
        => Task.FromResult(Enum.GetValues<BookCondition>()
            .Select(b => new LookupResponse { Id = (int)b, Name = b.ToString() })
            .ToList());

    public Task<List<LookupResponse>> GetMovementTypesAsync()
        => Task.FromResult(Enum.GetValues<MovementType>()
            .Select(m => new LookupResponse { Id = (int)m, Name = m.ToString() })
            .ToList());

    public async Task<List<StudentOperationsLookupResponse>> GetStudentsForOperationsAsync(int academicYearId, string? search = null, int take = 20)
    {
        take = Math.Clamp(take, 1, 100);

        var query = _studentRepo.Query()
            .Include(s => s.ClassSection).ThenInclude(cs => cs.Grade)
            .Include(s => s.StudentParents).ThenInclude(sp => sp.Parent)
            .Where(s => s.ClassSection.AcademicYearId == academicYearId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(s =>
                s.FullName.Contains(term) ||
                s.IndexNo.Contains(term) ||
                s.NationalId.Contains(term));
        }

        return await query
            .OrderBy(s => s.FullName)
            .Take(take)
            .Select(s => new StudentOperationsLookupResponse
            {
                Id = s.Id,
                FullName = s.FullName,
                IndexNo = s.IndexNo,
                NationalId = s.NationalId,
                ClassSectionId = s.ClassSectionId,
                ClassSectionName = s.ClassSection.Grade.Name + " " + s.ClassSection.Section,
                PrimaryParent = s.StudentParents
                    .OrderByDescending(sp => sp.IsPrimary)
                    .Select(sp => new ParentLookupSummaryResponse
                    {
                        Id = sp.Parent.Id,
                        FullName = sp.Parent.FullName,
                        NationalId = sp.Parent.NationalId,
                        Phone = sp.Parent.Phone,
                        Relationship = sp.Parent.Relationship
                    })
                    .FirstOrDefault()
            })
            .ToListAsync();
    }

    public async Task<List<ParentLookupSummaryResponse>> GetParentsForOperationsAsync(int studentId, string? search = null, int take = 20)
    {
        take = Math.Clamp(take, 1, 100);

        var query = _parentRepo.Query()
            .Where(p => p.StudentParents.Any(sp => sp.StudentId == studentId))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p =>
                p.FullName.Contains(term) ||
                p.NationalId.Contains(term) ||
                (p.Phone != null && p.Phone.Contains(term)));
        }

        return await query
            .OrderBy(p => p.FullName)
            .Take(take)
            .Select(p => new ParentLookupSummaryResponse
            {
                Id = p.Id,
                FullName = p.FullName,
                NationalId = p.NationalId,
                Phone = p.Phone,
                Relationship = p.Relationship
            })
            .ToListAsync();
    }

    public async Task<List<BookOperationsLookupResponse>> GetBooksForOperationsAsync(int academicYearId, string? search = null, int take = 20)
    {
        take = Math.Clamp(take, 1, 100);

        var query = _bookRepo.Query()
            .Include(b => b.Subject)
            .Include(b => b.StockEntries)
            .Where(b => b.StockEntries.Any(se => se.AcademicYearId == academicYearId))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(b =>
                b.Title.Contains(term) ||
                b.Code.Contains(term) ||
                (b.ISBN != null && b.ISBN.Contains(term)));
        }

        return await query
            .OrderBy(b => b.Title)
            .Take(take)
            .Select(b => new BookOperationsLookupResponse
            {
                Id = b.Id,
                Code = b.Code,
                Title = b.Title,
                SubjectName = b.Subject.Name,
                AvailableStock = b.TotalStock - b.Distributed - b.WithTeachers - b.Damaged - b.Lost
            })
            .ToListAsync();
    }

    public async Task<List<TeacherOperationsLookupResponse>> GetTeachersForOperationsAsync(int? academicYearId = null, string? search = null, int take = 20)
    {
        take = Math.Clamp(take, 1, 100);

        var query = _teacherRepo.Query()
            .Include(t => t.TeacherAssignments)
                .ThenInclude(a => a.Subject)
            .Include(t => t.TeacherAssignments)
                .ThenInclude(a => a.ClassSection)
                .ThenInclude(cs => cs.Grade)
            .AsQueryable();

        if (academicYearId.HasValue)
        {
            var yearId = academicYearId.Value;
            query = query.Where(t => t.TeacherAssignments.Any(a => a.ClassSection.AcademicYearId == yearId));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(t =>
                t.FullName.Contains(term) ||
                t.NationalId.Contains(term) ||
                (t.Email != null && t.Email.Contains(term)));
        }

        return await query
            .OrderBy(t => t.FullName)
            .Take(take)
            .Select(t => new TeacherOperationsLookupResponse
            {
                Id = t.Id,
                FullName = t.FullName,
                NationalId = t.NationalId,
                Email = t.Email,
                Phone = t.Phone,
                Assignments = t.TeacherAssignments
                    .OrderBy(a => a.ClassSection.Grade.SortOrder)
                    .ThenBy(a => a.ClassSection.Section)
                    .Select(a => a.Subject.Name + " (" + a.ClassSection.Grade.Name + " " + a.ClassSection.Section + ")")
                    .ToList()
            })
            .ToListAsync();
    }

    public async Task<List<TeacherIssueOutstandingLookupResponse>> GetTeacherIssueOutstandingForOperationsAsync(
        int? academicYearId = null,
        int? teacherId = null,
        string? search = null,
        int take = 20)
    {
        take = Math.Clamp(take, 1, 100);

        var query = _teacherIssueRepo.Query()
            .Include(i => i.AcademicYear)
            .Include(i => i.Teacher)
            .Include(i => i.Items).ThenInclude(ii => ii.Book)
            .Where(i => i.LifecycleStatus != SlipLifecycleStatus.Cancelled)
            .Where(i => i.Items.Any(ii => ii.OutstandingQuantity > 0))
            .AsQueryable();

        if (academicYearId.HasValue)
            query = query.Where(i => i.AcademicYearId == academicYearId.Value);

        if (teacherId.HasValue)
            query = query.Where(i => i.TeacherId == teacherId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(i =>
                i.ReferenceNo.Contains(term) ||
                i.Teacher.FullName.Contains(term) ||
                i.Teacher.NationalId.Contains(term));
        }

        return await query
            .OrderByDescending(i => i.IssuedAt)
            .Take(take)
            .Select(i => new TeacherIssueOutstandingLookupResponse
            {
                IssueId = i.Id,
                ReferenceNo = i.ReferenceNo,
                AcademicYearId = i.AcademicYearId,
                AcademicYearName = i.AcademicYear.Name,
                TeacherId = i.TeacherId,
                TeacherName = i.Teacher.FullName,
                Items = i.Items
                    .Where(ii => ii.OutstandingQuantity > 0)
                    .Select(ii => new TeacherIssueOutstandingItemResponse
                    {
                        TeacherIssueItemId = ii.Id,
                        BookId = ii.BookId,
                        BookCode = ii.Book.Code,
                        BookTitle = ii.Book.Title,
                        OutstandingQuantity = ii.OutstandingQuantity
                    })
                    .ToList()
            })
            .ToListAsync();
    }
}
