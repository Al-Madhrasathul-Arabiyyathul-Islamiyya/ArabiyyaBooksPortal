using BooksPortal.Application.Features.MasterData.DTOs;

namespace BooksPortal.Application.Features.MasterData.Interfaces;

public interface ISubjectService
{
    Task<List<SubjectResponse>> GetAllAsync();
    Task<SubjectResponse> GetByIdAsync(int id);
    Task<SubjectResponse> CreateAsync(CreateSubjectRequest request);
    Task<SubjectResponse> UpdateAsync(int id, CreateSubjectRequest request);
    Task DeleteAsync(int id);
}
