using BooksPortal.Application.Features.MasterData.DTOs;

namespace BooksPortal.Application.Features.MasterData.Interfaces;

public interface IGradeService
{
    Task<List<GradeResponse>> GetAllAsync(int? keystageId = null);
    Task<GradeResponse> GetByIdAsync(int id);
    Task<GradeResponse> CreateAsync(CreateGradeRequest request);
    Task<GradeResponse> UpdateAsync(int id, CreateGradeRequest request);
    Task DeleteAsync(int id);
}
