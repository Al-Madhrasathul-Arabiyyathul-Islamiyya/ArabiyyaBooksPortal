using BooksPortal.Application.Common.Exceptions;
using BooksPortal.Application.Common.Interfaces;
using BooksPortal.Application.Features.MasterData.DTOs;
using BooksPortal.Application.Features.MasterData.Interfaces;
using BooksPortal.Domain.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace BooksPortal.Application.Features.MasterData.Services;

public class KeystageService : IKeystageService
{
    private readonly IRepository<Keystage> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public KeystageService(IRepository<Keystage> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<KeystageResponse>> GetAllAsync()
    {
        var items = await _repository.Query().OrderBy(k => k.SortOrder).ToListAsync();
        return items.Adapt<List<KeystageResponse>>();
    }

    public async Task<KeystageResponse> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Keystage), id);
        return entity.Adapt<KeystageResponse>();
    }

    public async Task<KeystageResponse> CreateAsync(CreateKeystageRequest request)
    {
        if (await _repository.AnyAsync(k => k.Code == request.Code))
            throw new BusinessRuleException($"Keystage with code '{request.Code}' already exists.");

        var entity = request.Adapt<Keystage>();
        _repository.Add(entity);
        await _unitOfWork.SaveChangesAsync();
        return entity.Adapt<KeystageResponse>();
    }

    public async Task<KeystageResponse> UpdateAsync(int id, CreateKeystageRequest request)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Keystage), id);

        if (await _repository.AnyAsync(k => k.Code == request.Code && k.Id != id))
            throw new BusinessRuleException($"Keystage with code '{request.Code}' already exists.");

        request.Adapt(entity);
        _repository.Update(entity);
        await _unitOfWork.SaveChangesAsync();
        return entity.Adapt<KeystageResponse>();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Keystage), id);
        _repository.SoftDelete(entity);
        await _unitOfWork.SaveChangesAsync();
    }
}
