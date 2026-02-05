using BooksPortal.Application.Common.Models;
using BooksPortal.Application.Features.MasterData.DTOs;

namespace BooksPortal.Application.Features.MasterData.Interfaces;

public interface ITeacherService
{
    Task<PaginatedList<TeacherResponse>> GetPagedAsync(int pageNumber, int pageSize, string? search = null);
    Task<TeacherResponse> GetByIdAsync(int id);
    Task<TeacherResponse> CreateAsync(CreateTeacherRequest request);
    Task<TeacherResponse> UpdateAsync(int id, CreateTeacherRequest request);
    Task DeleteAsync(int id);
    Task<TeacherAssignmentResponse> AddAssignmentAsync(int teacherId, CreateTeacherAssignmentRequest request);
    Task RemoveAssignmentAsync(int teacherId, int assignmentId);
}
