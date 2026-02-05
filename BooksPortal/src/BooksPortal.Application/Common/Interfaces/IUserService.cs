using BooksPortal.Application.Features.Users.DTOs;

namespace BooksPortal.Application.Common.Interfaces;

public interface IUserService
{
    Task<List<UserResponse>> GetAllUsersAsync();
    Task<UserResponse> GetUserByIdAsync(int id);
    Task<UserResponse> CreateUserAsync(CreateUserRequest request);
    Task<UserResponse> UpdateUserAsync(int id, UpdateUserRequest request);
    Task ToggleUserActiveAsync(int id);
    Task AssignRolesAsync(int id, List<string> roles);
}
