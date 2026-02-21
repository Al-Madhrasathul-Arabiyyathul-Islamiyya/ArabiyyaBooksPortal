using BooksPortal.Application.Common.Exceptions;
using BooksPortal.Application.Common.Interfaces;
using BooksPortal.Application.Features.Settings.DTOs;
using BooksPortal.Application.Features.Settings.Interfaces;
using BooksPortal.Domain.Entities;
using BooksPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BooksPortal.Application.Features.Settings.Services;

public class ReferenceNumberFormatService : IReferenceNumberFormatService
{
    private readonly IRepository<ReferenceNumberFormat> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ReferenceNumberFormatService(IRepository<ReferenceNumberFormat> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<ReferenceNumberFormatResponse>> GetAllAsync(SlipType? slipType = null, int? academicYearId = null)
    {
        var query = _repository.Query()
            .Include(f => f.AcademicYear)
            .AsQueryable();

        if (slipType.HasValue)
            query = query.Where(f => f.SlipType == slipType.Value);

        if (academicYearId.HasValue)
            query = query.Where(f => f.AcademicYearId == academicYearId.Value);

        var items = await query.OrderBy(f => f.SlipType).ThenByDescending(f => f.AcademicYear.Year).ToListAsync();

        return items.Select(MapToResponse).ToList();
    }

    public async Task<ReferenceNumberFormatResponse> GetByIdAsync(int id)
    {
        var entity = await _repository.Query()
            .Include(f => f.AcademicYear)
            .FirstOrDefaultAsync(f => f.Id == id)
            ?? throw new NotFoundException(nameof(ReferenceNumberFormat), id);

        return MapToResponse(entity);
    }

    public async Task<ReferenceNumberFormatResponse> CreateAsync(CreateReferenceNumberFormatRequest request)
    {
        var existing = await _repository.QueryIgnoringFilters()
            .FirstOrDefaultAsync(f => f.SlipType == request.SlipType && f.AcademicYearId == request.AcademicYearId);

        if (existing is not null && !existing.IsDeleted)
            throw new BusinessRuleException($"A format already exists for {request.SlipType} in this academic year.");

        if (existing is not null && existing.IsDeleted)
        {
            existing.IsDeleted = false;
            existing.DeletedAt = null;
            existing.FormatTemplate = request.FormatTemplate;
            existing.PaddingWidth = request.PaddingWidth;

            _repository.Update(existing);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(existing.Id);
        }

        var entity = new ReferenceNumberFormat
        {
            SlipType = request.SlipType,
            AcademicYearId = request.AcademicYearId,
            FormatTemplate = request.FormatTemplate,
            PaddingWidth = request.PaddingWidth
        };

        _repository.Add(entity);
        await _unitOfWork.SaveChangesAsync();

        return await GetByIdAsync(entity.Id);
    }

    public async Task<ReferenceNumberFormatResponse> UpdateAsync(int id, CreateReferenceNumberFormatRequest request)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(ReferenceNumberFormat), id);

        var conflicting = await _repository.QueryIgnoringFilters()
            .AnyAsync(f => f.SlipType == request.SlipType && f.AcademicYearId == request.AcademicYearId && f.Id != id);

        if (conflicting)
            throw new BusinessRuleException($"A format already exists for {request.SlipType} in this academic year.");

        entity.SlipType = request.SlipType;
        entity.AcademicYearId = request.AcademicYearId;
        entity.FormatTemplate = request.FormatTemplate;
        entity.PaddingWidth = request.PaddingWidth;

        _repository.Update(entity);
        await _unitOfWork.SaveChangesAsync();

        return await GetByIdAsync(entity.Id);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(ReferenceNumberFormat), id);
        _repository.SoftDelete(entity);
        await _unitOfWork.SaveChangesAsync();
    }

    private static ReferenceNumberFormatResponse MapToResponse(ReferenceNumberFormat entity) => new()
    {
        Id = entity.Id,
        SlipType = entity.SlipType,
        AcademicYearId = entity.AcademicYearId,
        AcademicYearName = entity.AcademicYear.Name,
        FormatTemplate = entity.FormatTemplate,
        PaddingWidth = entity.PaddingWidth
    };
}
