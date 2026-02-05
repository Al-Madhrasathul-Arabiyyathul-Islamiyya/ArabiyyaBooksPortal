using BooksPortal.Application.Common.Exceptions;
using BooksPortal.Application.Common.Interfaces;
using BooksPortal.Application.Features.MasterData.DTOs;
using BooksPortal.Application.Features.MasterData.Interfaces;
using BooksPortal.Domain.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace BooksPortal.Application.Features.MasterData.Services;

public class SubjectService : ISubjectService
{
    private readonly IRepository<Subject> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public SubjectService(IRepository<Subject> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<SubjectResponse>> GetAllAsync()
    {
        var items = await _repository.Query().OrderBy(s => s.Name).ToListAsync();
        return items.Adapt<List<SubjectResponse>>();
    }

    public async Task<SubjectResponse> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Subject), id);
        return entity.Adapt<SubjectResponse>();
    }

    public async Task<SubjectResponse> CreateAsync(CreateSubjectRequest request)
    {
        if (await _repository.AnyAsync(s => s.Code == request.Code))
            throw new BusinessRuleException($"Subject with code '{request.Code}' already exists.");

        var entity = request.Adapt<Subject>();
        _repository.Add(entity);
        await _unitOfWork.SaveChangesAsync();
        return entity.Adapt<SubjectResponse>();
    }

    public async Task<SubjectResponse> UpdateAsync(int id, CreateSubjectRequest request)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Subject), id);

        if (await _repository.AnyAsync(s => s.Code == request.Code && s.Id != id))
            throw new BusinessRuleException($"Subject with code '{request.Code}' already exists.");

        request.Adapt(entity);
        _repository.Update(entity);
        await _unitOfWork.SaveChangesAsync();
        return entity.Adapt<SubjectResponse>();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Subject), id);
        _repository.SoftDelete(entity);
        await _unitOfWork.SaveChangesAsync();
    }
}
