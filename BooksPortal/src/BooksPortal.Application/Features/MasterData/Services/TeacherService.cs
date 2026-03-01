using BooksPortal.Application.Common.Exceptions;
using BooksPortal.Application.Common.Interfaces;
using BooksPortal.Application.Common.Models;
using BooksPortal.Application.Features.MasterData.DTOs;
using BooksPortal.Application.Features.MasterData.Interfaces;
using BooksPortal.Domain.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace BooksPortal.Application.Features.MasterData.Services;

public class TeacherService : ITeacherService
{
    private readonly IRepository<Teacher> _repository;
    private readonly IRepository<TeacherAssignment> _assignmentRepo;
    private readonly IUnitOfWork _unitOfWork;

    public TeacherService(
        IRepository<Teacher> repository,
        IRepository<TeacherAssignment> assignmentRepo,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _assignmentRepo = assignmentRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<PaginatedList<TeacherResponse>> GetPagedAsync(int pageNumber, int pageSize, string? search = null)
    {
        var query = _repository.Query()
            .Include(t => t.TeacherAssignments).ThenInclude(ta => ta.Subject)
            .Include(t => t.TeacherAssignments).ThenInclude(ta => ta.ClassSection).ThenInclude(cs => cs.Grade)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(t => t.FullName.Contains(search) || t.NationalId.Contains(search));

        var projected = query.OrderBy(t => t.FullName).Select(t => new TeacherResponse
        {
            Id = t.Id,
            FullName = t.FullName,
            NationalId = t.NationalId,
            Email = t.Email,
            Phone = t.Phone,
            Assignments = t.TeacherAssignments.Select(ta => new TeacherAssignmentResponse
            {
                Id = ta.Id,
                SubjectId = ta.SubjectId,
                SubjectName = ta.Subject.Name,
                ClassSectionId = ta.ClassSectionId,
                ClassSectionDisplayName = ta.ClassSection.Grade.Name + " " + ta.ClassSection.Section
            }).ToList()
        });

        return await PaginatedList<TeacherResponse>.CreateAsync(projected, pageNumber, pageSize);
    }

    public async Task<TeacherResponse> GetByIdAsync(int id)
    {
        var entity = await _repository.Query()
            .Include(t => t.TeacherAssignments).ThenInclude(ta => ta.Subject)
            .Include(t => t.TeacherAssignments).ThenInclude(ta => ta.ClassSection).ThenInclude(cs => cs.Grade)
            .FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new NotFoundException(nameof(Teacher), id);

        return new TeacherResponse
        {
            Id = entity.Id,
            FullName = entity.FullName,
            NationalId = entity.NationalId,
            Email = entity.Email,
            Phone = entity.Phone,
            Assignments = entity.TeacherAssignments.Select(ta => new TeacherAssignmentResponse
            {
                Id = ta.Id,
                SubjectId = ta.SubjectId,
                SubjectName = ta.Subject.Name,
                ClassSectionId = ta.ClassSectionId,
                ClassSectionDisplayName = ta.ClassSection.Grade.Name + " " + ta.ClassSection.Section
            }).ToList()
        };
    }

    public async Task<TeacherResponse> CreateAsync(CreateTeacherRequest request)
    {
        if (await _repository.AnyAsync(t => t.NationalId == request.NationalId))
            throw new BusinessRuleException($"Teacher with national ID '{request.NationalId}' already exists.");

        var entity = request.Adapt<Teacher>();
        _repository.Add(entity);
        await _unitOfWork.SaveChangesAsync();
        return await GetByIdAsync(entity.Id);
    }

    public async Task<TeacherResponse> UpdateAsync(int id, CreateTeacherRequest request)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Teacher), id);

        if (await _repository.AnyAsync(t => t.NationalId == request.NationalId && t.Id != id))
            throw new BusinessRuleException($"Teacher with national ID '{request.NationalId}' already exists.");

        entity.FullName = request.FullName;
        entity.NationalId = request.NationalId;
        entity.Email = request.Email;
        entity.Phone = request.Phone;
        _repository.Update(entity);
        await _unitOfWork.SaveChangesAsync();
        return await GetByIdAsync(entity.Id);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Teacher), id);
        _repository.SoftDelete(entity);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<TeacherAssignmentResponse> AddAssignmentAsync(int teacherId, CreateTeacherAssignmentRequest request)
    {
        if (!await _repository.AnyAsync(t => t.Id == teacherId))
            throw new NotFoundException(nameof(Teacher), teacherId);

        if (await _assignmentRepo.AnyAsync(ta =>
            ta.TeacherId == teacherId &&
            ta.SubjectId == request.SubjectId &&
            ta.ClassSectionId == request.ClassSectionId))
            throw new BusinessRuleException("This assignment already exists.");

        var assignment = new TeacherAssignment
        {
            TeacherId = teacherId,
            SubjectId = request.SubjectId,
            ClassSectionId = request.ClassSectionId
        };

        _assignmentRepo.Add(assignment);
        await _unitOfWork.SaveChangesAsync();

        var saved = await _assignmentRepo.Query()
            .Include(ta => ta.Subject)
            .Include(ta => ta.ClassSection).ThenInclude(cs => cs.Grade)
            .FirstAsync(ta => ta.Id == assignment.Id);

        return new TeacherAssignmentResponse
        {
            Id = saved.Id,
            SubjectId = saved.SubjectId,
            SubjectName = saved.Subject.Name,
            ClassSectionId = saved.ClassSectionId,
            ClassSectionDisplayName = saved.ClassSection.Grade.Name + " " + saved.ClassSection.Section
        };
    }

    public async Task RemoveAssignmentAsync(int teacherId, int assignmentId)
    {
        var assignment = await _assignmentRepo.GetByIdAsync(assignmentId)
            ?? throw new NotFoundException(nameof(TeacherAssignment), assignmentId);

        if (assignment.TeacherId != teacherId)
            throw new BusinessRuleException("Assignment does not belong to this teacher.");

        _assignmentRepo.SoftDelete(assignment);
        await _unitOfWork.SaveChangesAsync();
    }
}
