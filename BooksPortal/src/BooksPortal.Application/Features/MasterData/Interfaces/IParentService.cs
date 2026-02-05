using BooksPortal.Application.Common.Models;
using BooksPortal.Application.Features.MasterData.DTOs;

namespace BooksPortal.Application.Features.MasterData.Interfaces;

public interface IParentService
{
    Task<PaginatedList<ParentResponse>> GetPagedAsync(int pageNumber, int pageSize, string? search = null);
    Task<ParentResponse> GetByIdAsync(int id);
    Task<ParentResponse> CreateAsync(CreateParentRequest request);
    Task<ParentResponse> UpdateAsync(int id, CreateParentRequest request);
    Task DeleteAsync(int id);
}
