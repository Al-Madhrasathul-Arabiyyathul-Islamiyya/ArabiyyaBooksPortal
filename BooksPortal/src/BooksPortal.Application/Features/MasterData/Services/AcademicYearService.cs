using BooksPortal.Application.Common.Exceptions;
using BooksPortal.Application.Common.Interfaces;
using BooksPortal.Application.Features.MasterData.DTOs;
using BooksPortal.Application.Features.MasterData.Interfaces;
using BooksPortal.Domain.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace BooksPortal.Application.Features.MasterData.Services;

public class AcademicYearService : IAcademicYearService
{
    private readonly IRepository<AcademicYear> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public AcademicYearService(IRepository<AcademicYear> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<AcademicYearResponse>> GetAllAsync()
    {
        var items = await _repository.Query().OrderByDescending(a => a.Year).ToListAsync();
        return items.Adapt<List<AcademicYearResponse>>();
    }

    public async Task<AcademicYearResponse> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(AcademicYear), id);
        return entity.Adapt<AcademicYearResponse>();
    }

    public async Task<AcademicYearResponse> CreateAsync(CreateAcademicYearRequest request)
    {
        if (await _repository.AnyAsync(a => a.Year == request.Year))
            throw new BusinessRuleException($"Academic year {request.Year} already exists.");

        var entity = request.Adapt<AcademicYear>();
        _repository.Add(entity);
        await _unitOfWork.SaveChangesAsync();
        return entity.Adapt<AcademicYearResponse>();
    }

    public async Task<AcademicYearResponse> UpdateAsync(int id, UpdateAcademicYearRequest request)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(AcademicYear), id);

        if (await _repository.AnyAsync(a => a.Year == request.Year && a.Id != id))
            throw new BusinessRuleException($"Academic year {request.Year} already exists.");

        request.Adapt(entity);
        _repository.Update(entity);
        await _unitOfWork.SaveChangesAsync();
        return entity.Adapt<AcademicYearResponse>();
    }

    public async Task ActivateAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(AcademicYear), id);

        var allYears = await _repository.GetAllAsync();
        foreach (var year in allYears)
        {
            year.IsActive = year.Id == id;
            _repository.Update(year);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(AcademicYear), id);
        _repository.SoftDelete(entity);
        await _unitOfWork.SaveChangesAsync();
    }
}
