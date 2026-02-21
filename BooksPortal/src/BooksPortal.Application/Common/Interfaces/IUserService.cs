using BooksPortal.Application.Common.Models;
using BooksPortal.Application.Features.Users.DTOs;

namespace BooksPortal.Application.Common.Interfaces;

public interface IUserService
{
    Task<PaginatedList<UserResponse>> GetPagedUsersAsync(
        int pageNumber,
        int pageSize,
        string? search = null,
        bool? isActive = null,
        string? role = null);
    Task<UserResponse> GetUserByIdAsync(int id);
    Task<UserResponse> CreateUserAsync(CreateUserRequest request);
    Task<UserResponse> UpdateUserAsync(int id, UpdateUserRequest request);
    Task ToggleUserActiveAsync(int id);
    Task AssignRolesAsync(int id, List<string> roles);
}
