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

    public LookupService(
        IRepository<AcademicYear> academicYearRepo,
        IRepository<Keystage> keystageRepo,
        IRepository<Grade> gradeRepo,
        IRepository<Subject> subjectRepo,
        IRepository<ClassSection> classSectionRepo)
    {
        _academicYearRepo = academicYearRepo;
        _keystageRepo = keystageRepo;
        _gradeRepo = gradeRepo;
        _subjectRepo = subjectRepo;
        _classSectionRepo = classSectionRepo;
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
}
