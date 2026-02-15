using BooksPortal.Application.Common.Exceptions;
using BooksPortal.Application.Common.Interfaces;
using BooksPortal.Application.Common.Models;
using BooksPortal.Application.Features.MasterData.DTOs;
using BooksPortal.Application.Features.MasterData.Interfaces;
using BooksPortal.Domain.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace BooksPortal.Application.Features.MasterData.Services;

public class ClassSectionService : IClassSectionService
{
    private readonly IRepository<ClassSection> _repository;
    private readonly IRepository<Grade> _gradeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ClassSectionService(
        IRepository<ClassSection> repository,
        IRepository<Grade> gradeRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _gradeRepository = gradeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PaginatedList<ClassSectionResponse>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        int? academicYearId = null,
        int? keystageId = null,
        int? gradeId = null,
        string? search = null)
    {
        var query = _repository.Query()
            .Include(c => c.AcademicYear)
            .Include(c => c.Keystage)
            .Include(c => c.Grade)
            .AsQueryable();

        if (academicYearId.HasValue)
            query = query.Where(c => c.AcademicYearId == academicYearId.Value);

        if (keystageId.HasValue)
            query = query.Where(c => c.KeystageId == keystageId.Value);

        if (gradeId.HasValue)
            query = query.Where(c => c.GradeId == gradeId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(c =>
                c.Grade.Name.Contains(term) ||
                c.Section.Contains(term) ||
                c.Keystage.Name.Contains(term));
        }

        var projected = query
            .OrderBy(c => c.Grade.SortOrder)
            .ThenBy(c => c.Section)
            .Select(c => new ClassSectionResponse
        {
            Id = c.Id,
            AcademicYearId = c.AcademicYearId,
            AcademicYearName = c.AcademicYear.Name,
            KeystageId = c.KeystageId,
            KeystageName = c.Keystage.Name,
            GradeId = c.GradeId,
            Grade = c.Grade.Name,
            Section = c.Section,
            StudentCount = c.Students.Count
        });

        return await PaginatedList<ClassSectionResponse>.CreateAsync(projected, pageNumber, pageSize);
    }

    public async Task<ClassSectionResponse> GetByIdAsync(int id)
    {
        var entity = await _repository.Query()
            .Include(c => c.AcademicYear)
            .Include(c => c.Keystage)
            .Include(c => c.Grade)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new NotFoundException(nameof(ClassSection), id);

        return new ClassSectionResponse
        {
            Id = entity.Id,
            AcademicYearId = entity.AcademicYearId,
            AcademicYearName = entity.AcademicYear.Name,
            KeystageId = entity.KeystageId,
            KeystageName = entity.Keystage.Name,
            GradeId = entity.GradeId,
            Grade = entity.Grade.Name,
            Section = entity.Section
        };
    }

    public async Task<ClassSectionResponse> CreateAsync(CreateClassSectionRequest request)
    {
        var grade = await _gradeRepository.GetByIdAsync(request.GradeId)
            ?? throw new NotFoundException(nameof(Grade), request.GradeId);

        if (grade.KeystageId != request.KeystageId)
            throw new BusinessRuleException("Selected grade does not belong to the selected keystage.");

        if (await _repository.AnyAsync(c =>
            c.AcademicYearId == request.AcademicYearId &&
            c.GradeId == request.GradeId &&
            c.Section == request.Section))
            throw new BusinessRuleException($"Class section {grade.Name} {request.Section} already exists for this academic year.");

        var entity = request.Adapt<ClassSection>();
        _repository.Add(entity);
        await _unitOfWork.SaveChangesAsync();
        return await GetByIdAsync(entity.Id);
    }

    public async Task<ClassSectionResponse> UpdateAsync(int id, CreateClassSectionRequest request)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(ClassSection), id);

        var grade = await _gradeRepository.GetByIdAsync(request.GradeId)
            ?? throw new NotFoundException(nameof(Grade), request.GradeId);

        if (grade.KeystageId != request.KeystageId)
            throw new BusinessRuleException("Selected grade does not belong to the selected keystage.");

        if (await _repository.AnyAsync(c =>
            c.AcademicYearId == request.AcademicYearId &&
            c.GradeId == request.GradeId &&
            c.Section == request.Section &&
            c.Id != id))
            throw new BusinessRuleException($"Class section {grade.Name} {request.Section} already exists for this academic year.");

        request.Adapt(entity);
        _repository.Update(entity);
        await _unitOfWork.SaveChangesAsync();
        return await GetByIdAsync(entity.Id);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(ClassSection), id);
        _repository.SoftDelete(entity);
        await _unitOfWork.SaveChangesAsync();
    }
}
