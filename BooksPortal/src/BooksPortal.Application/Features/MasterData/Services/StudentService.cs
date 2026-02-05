using BooksPortal.Application.Common.Exceptions;
using BooksPortal.Application.Common.Interfaces;
using BooksPortal.Application.Common.Models;
using BooksPortal.Application.Features.MasterData.DTOs;
using BooksPortal.Application.Features.MasterData.Interfaces;
using BooksPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BooksPortal.Application.Features.MasterData.Services;

public class StudentService : IStudentService
{
    private readonly IRepository<Student> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public StudentService(
        IRepository<Student> repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PaginatedList<StudentResponse>> GetPagedAsync(int pageNumber, int pageSize, int? classSectionId = null, string? search = null)
    {
        var query = _repository.Query()
            .Include(s => s.ClassSection)
            .Include(s => s.StudentParents).ThenInclude(sp => sp.Parent)
            .AsQueryable();

        if (classSectionId.HasValue)
            query = query.Where(s => s.ClassSectionId == classSectionId.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(s => s.FullName.Contains(search) || s.IndexNo.Contains(search));

        var projected = query.OrderBy(s => s.FullName).Select(s => new StudentResponse
        {
            Id = s.Id,
            FullName = s.FullName,
            IndexNo = s.IndexNo,
            NationalId = s.NationalId,
            ClassSectionId = s.ClassSectionId,
            ClassSectionDisplayName = s.ClassSection.Grade + " " + s.ClassSection.Section,
            Parents = s.StudentParents.Select(sp => new StudentParentResponse
            {
                ParentId = sp.ParentId,
                FullName = sp.Parent.FullName,
                NationalId = sp.Parent.NationalId,
                Phone = sp.Parent.Phone,
                Relationship = sp.Parent.Relationship,
                IsPrimary = sp.IsPrimary
            }).ToList()
        });

        return await PaginatedList<StudentResponse>.CreateAsync(projected, pageNumber, pageSize);
    }

    public async Task<StudentResponse> GetByIdAsync(int id)
    {
        var entity = await _repository.Query()
            .Include(s => s.ClassSection)
            .Include(s => s.StudentParents).ThenInclude(sp => sp.Parent)
            .FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new NotFoundException(nameof(Student), id);

        return new StudentResponse
        {
            Id = entity.Id,
            FullName = entity.FullName,
            IndexNo = entity.IndexNo,
            NationalId = entity.NationalId,
            ClassSectionId = entity.ClassSectionId,
            ClassSectionDisplayName = entity.ClassSection.Grade + " " + entity.ClassSection.Section,
            Parents = entity.StudentParents.Select(sp => new StudentParentResponse
            {
                ParentId = sp.ParentId,
                FullName = sp.Parent.FullName,
                NationalId = sp.Parent.NationalId,
                Phone = sp.Parent.Phone,
                Relationship = sp.Parent.Relationship,
                IsPrimary = sp.IsPrimary
            }).ToList()
        };
    }

    public async Task<StudentResponse> CreateAsync(CreateStudentRequest request)
    {
        if (await _repository.AnyAsync(s => s.IndexNo == request.IndexNo))
            throw new BusinessRuleException($"Student with index number '{request.IndexNo}' already exists.");

        var entity = new Student
        {
            FullName = request.FullName,
            IndexNo = request.IndexNo,
            NationalId = request.NationalId,
            ClassSectionId = request.ClassSectionId
        };

        if (request.Parents?.Any() == true)
        {
            foreach (var p in request.Parents)
                entity.StudentParents.Add(new StudentParent { ParentId = p.ParentId, IsPrimary = p.IsPrimary });
        }

        _repository.Add(entity);
        await _unitOfWork.SaveChangesAsync();
        return await GetByIdAsync(entity.Id);
    }

    public async Task<StudentResponse> UpdateAsync(int id, UpdateStudentRequest request)
    {
        var entity = await _repository.Query()
            .Include(s => s.StudentParents)
            .FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new NotFoundException(nameof(Student), id);

        entity.FullName = request.FullName;
        entity.NationalId = request.NationalId;
        entity.ClassSectionId = request.ClassSectionId;

        if (request.Parents != null)
        {
            entity.StudentParents.Clear();
            foreach (var p in request.Parents)
                entity.StudentParents.Add(new StudentParent { StudentId = id, ParentId = p.ParentId, IsPrimary = p.IsPrimary });
        }

        _repository.Update(entity);
        await _unitOfWork.SaveChangesAsync();
        return await GetByIdAsync(entity.Id);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Student), id);
        _repository.SoftDelete(entity);
        await _unitOfWork.SaveChangesAsync();
    }
}
