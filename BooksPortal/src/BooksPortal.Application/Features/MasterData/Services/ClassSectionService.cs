using BooksPortal.Application.Common.Exceptions;
using BooksPortal.Application.Common.Interfaces;
using BooksPortal.Application.Features.MasterData.DTOs;
using BooksPortal.Application.Features.MasterData.Interfaces;
using BooksPortal.Domain.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace BooksPortal.Application.Features.MasterData.Services;

public class ClassSectionService : IClassSectionService
{
    private readonly IRepository<ClassSection> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ClassSectionService(IRepository<ClassSection> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<ClassSectionResponse>> GetAllAsync(int? academicYearId = null)
    {
        var query = _repository.Query()
            .Include(c => c.AcademicYear)
            .Include(c => c.Keystage)
            .AsQueryable();

        if (academicYearId.HasValue)
            query = query.Where(c => c.AcademicYearId == academicYearId.Value);

        var items = await query.OrderBy(c => c.Grade).ThenBy(c => c.Section).ToListAsync();
        return items.Select(c => new ClassSectionResponse
        {
            Id = c.Id,
            AcademicYearId = c.AcademicYearId,
            AcademicYearName = c.AcademicYear.Name,
            KeystageId = c.KeystageId,
            KeystageName = c.Keystage.Name,
            Grade = c.Grade,
            Section = c.Section,
            StudentCount = c.Students.Count
        }).ToList();
    }

    public async Task<ClassSectionResponse> GetByIdAsync(int id)
    {
        var entity = await _repository.Query()
            .Include(c => c.AcademicYear)
            .Include(c => c.Keystage)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new NotFoundException(nameof(ClassSection), id);

        return new ClassSectionResponse
        {
            Id = entity.Id,
            AcademicYearId = entity.AcademicYearId,
            AcademicYearName = entity.AcademicYear.Name,
            KeystageId = entity.KeystageId,
            KeystageName = entity.Keystage.Name,
            Grade = entity.Grade,
            Section = entity.Section
        };
    }

    public async Task<ClassSectionResponse> CreateAsync(CreateClassSectionRequest request)
    {
        if (await _repository.AnyAsync(c =>
            c.AcademicYearId == request.AcademicYearId &&
            c.Grade == request.Grade &&
            c.Section == request.Section))
            throw new BusinessRuleException($"Class section {request.Grade} {request.Section} already exists for this academic year.");

        var entity = request.Adapt<ClassSection>();
        _repository.Add(entity);
        await _unitOfWork.SaveChangesAsync();
        return await GetByIdAsync(entity.Id);
    }

    public async Task<ClassSectionResponse> UpdateAsync(int id, CreateClassSectionRequest request)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(ClassSection), id);

        if (await _repository.AnyAsync(c =>
            c.AcademicYearId == request.AcademicYearId &&
            c.Grade == request.Grade &&
            c.Section == request.Section &&
            c.Id != id))
            throw new BusinessRuleException($"Class section {request.Grade} {request.Section} already exists for this academic year.");

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
