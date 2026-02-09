using BooksPortal.Application.Common.Exceptions;
using BooksPortal.Application.Common.Interfaces;
using BooksPortal.Application.Common.Models;
using BooksPortal.Application.Features.MasterData.DTOs;
using BooksPortal.Application.Features.MasterData.Interfaces;
using BooksPortal.Domain.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace BooksPortal.Application.Features.MasterData.Services;

public class ParentService : IParentService
{
    private readonly IRepository<Parent> _repository;
    private readonly IRepository<Student> _studentRepository;
    private readonly IRepository<DistributionSlip> _distributionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ParentService(
        IRepository<Parent> repository,
        IRepository<Student> studentRepository,
        IRepository<DistributionSlip> distributionRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _studentRepository = studentRepository;
        _distributionRepository = distributionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PaginatedList<ParentResponse>> GetPagedAsync(int pageNumber, int pageSize, string? search = null)
    {
        var query = _repository.Query();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.FullName.Contains(search) || p.NationalId.Contains(search));

        var projected = query.OrderBy(p => p.FullName).Select(p => new ParentResponse
        {
            Id = p.Id,
            FullName = p.FullName,
            NationalId = p.NationalId,
            Phone = p.Phone,
            Relationship = p.Relationship
        });

        return await PaginatedList<ParentResponse>.CreateAsync(projected, pageNumber, pageSize);
    }

    public async Task<ParentResponse> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Parent), id);
        return entity.Adapt<ParentResponse>();
    }

    public async Task<ParentResponse> CreateAsync(CreateParentRequest request)
    {
        if (await _repository.AnyAsync(p => p.NationalId == request.NationalId))
            throw new BusinessRuleException($"Parent with national ID '{request.NationalId}' already exists.");

        var entity = request.Adapt<Parent>();
        _repository.Add(entity);
        await _unitOfWork.SaveChangesAsync();
        return entity.Adapt<ParentResponse>();
    }

    public async Task<ParentResponse> UpdateAsync(int id, CreateParentRequest request)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Parent), id);

        if (await _repository.AnyAsync(p => p.NationalId == request.NationalId && p.Id != id))
            throw new BusinessRuleException($"Parent with national ID '{request.NationalId}' already exists.");

        request.Adapt(entity);
        _repository.Update(entity);
        await _unitOfWork.SaveChangesAsync();
        return entity.Adapt<ParentResponse>();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Parent), id);

        var hasStudentLinks = await _studentRepository.Query()
            .AnyAsync(s => s.StudentParents.Any(sp => sp.ParentId == id));
        var hasDistributions = await _distributionRepository.AnyAsync(d => d.ParentId == id);

        if (hasStudentLinks || hasDistributions)
            throw new BusinessRuleException("Cannot delete parent because it is referenced by existing records.");

        _repository.SoftDelete(entity);
        await _unitOfWork.SaveChangesAsync();
    }
}
