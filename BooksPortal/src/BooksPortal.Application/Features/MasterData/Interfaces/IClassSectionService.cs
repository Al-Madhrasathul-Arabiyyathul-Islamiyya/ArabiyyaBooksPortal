using BooksPortal.Application.Common.Models;
using BooksPortal.Application.Features.MasterData.DTOs;

namespace BooksPortal.Application.Features.MasterData.Interfaces;

public interface IClassSectionService
{
    Task<PaginatedList<ClassSectionResponse>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        int? academicYearId = null,
        int? keystageId = null,
        int? gradeId = null,
        string? search = null);
    Task<ClassSectionResponse> GetByIdAsync(int id);
    Task<ClassSectionResponse> CreateAsync(CreateClassSectionRequest request);
    Task<ClassSectionResponse> UpdateAsync(int id, CreateClassSectionRequest request);
    Task DeleteAsync(int id);
}
