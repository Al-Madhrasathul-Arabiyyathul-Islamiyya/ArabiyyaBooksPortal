using BooksPortal.Application.Common.Models;
using BooksPortal.Application.Features.MasterData.DTOs;

namespace BooksPortal.Application.Features.MasterData.Interfaces;

public interface IStudentService
{
    Task<PaginatedList<StudentResponse>> GetPagedAsync(int pageNumber, int pageSize, int? classSectionId = null, string? search = null);
    Task<StudentResponse> GetByIdAsync(int id);
    Task<StudentResponse> CreateAsync(CreateStudentRequest request);
    Task<StudentResponse> UpdateAsync(int id, UpdateStudentRequest request);
    Task DeleteAsync(int id);
}
