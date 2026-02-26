using BooksPortal.Application.Common.Exceptions;
using BooksPortal.Application.Common.Interfaces;
using BooksPortal.Application.Features.MasterData.DTOs;
using BooksPortal.Application.Features.MasterData.Interfaces;
using BooksPortal.Domain.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace BooksPortal.Application.Features.MasterData.Services;

public class GradeService : IGradeService
{
    private readonly IRepository<Grade> _gradeRepository;
    private readonly IRepository<Keystage> _keystageRepository;
    private readonly IUnitOfWork _unitOfWork;

    public GradeService(
        IRepository<Grade> gradeRepository,
        IRepository<Keystage> keystageRepository,
        IUnitOfWork unitOfWork)
    {
        _gradeRepository = gradeRepository;
        _keystageRepository = keystageRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<GradeResponse>> GetAllAsync(int? keystageId = null)
    {
        var query = _gradeRepository.Query()
            .Include(g => g.Keystage)
            .AsQueryable();

        if (keystageId.HasValue)
        {
            query = query.Where(g => g.KeystageId == keystageId.Value);
        }

        return await query
            .OrderBy(g => g.SortOrder)
            .ThenBy(g => g.Name)
            .Select(g => new GradeResponse
            {
                Id = g.Id,
                KeystageId = g.KeystageId,
                KeystageName = g.Keystage.Name,
                Code = g.Code,
                Name = g.Name,
                SortOrder = g.SortOrder
            })
            .ToListAsync();
    }

    public async Task<GradeResponse> GetByIdAsync(int id)
    {
        var entity = await _gradeRepository.Query()
            .Include(g => g.Keystage)
            .FirstOrDefaultAsync(g => g.Id == id)
            ?? throw new NotFoundException(nameof(Grade), id);

        return new GradeResponse
        {
            Id = entity.Id,
            KeystageId = entity.KeystageId,
            KeystageName = entity.Keystage.Name,
            Code = entity.Code,
            Name = entity.Name,
            SortOrder = entity.SortOrder
        };
    }

    public async Task<GradeResponse> CreateAsync(CreateGradeRequest request)
    {
        await EnsureKeystageExistsAsync(request.KeystageId);

        if (await _gradeRepository.AnyAsync(g => g.KeystageId == request.KeystageId && g.Code == request.Code))
        {
            throw new BusinessRuleException($"Grade with code '{request.Code}' already exists in selected keystage.");
        }

        var entity = request.Adapt<Grade>();
        _gradeRepository.Add(entity);
        await _unitOfWork.SaveChangesAsync();

        return await GetByIdAsync(entity.Id);
    }

    public async Task<GradeResponse> UpdateAsync(int id, CreateGradeRequest request)
    {
        await EnsureKeystageExistsAsync(request.KeystageId);

        var entity = await _gradeRepository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Grade), id);

        if (await _gradeRepository.AnyAsync(g =>
                g.KeystageId == request.KeystageId &&
                g.Code == request.Code &&
                g.Id != id))
        {
            throw new BusinessRuleException($"Grade with code '{request.Code}' already exists in selected keystage.");
        }

        request.Adapt(entity);
        _gradeRepository.Update(entity);
        await _unitOfWork.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _gradeRepository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Grade), id);

        _gradeRepository.SoftDelete(entity);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task EnsureKeystageExistsAsync(int keystageId)
    {
        if (!await _keystageRepository.AnyAsync(k => k.Id == keystageId))
        {
            throw new NotFoundException(nameof(Keystage), keystageId);
        }
    }
}
