using BooksPortal.Application.Features.MasterData.DTOs;

namespace BooksPortal.Application.Features.MasterData.Interfaces;

public interface IKeystageService
{
    Task<List<KeystageResponse>> GetAllAsync();
    Task<KeystageResponse> GetByIdAsync(int id);
    Task<KeystageResponse> CreateAsync(CreateKeystageRequest request);
    Task<KeystageResponse> UpdateAsync(int id, CreateKeystageRequest request);
    Task DeleteAsync(int id);
}
