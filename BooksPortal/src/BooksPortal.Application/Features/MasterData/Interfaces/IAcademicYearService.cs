using BooksPortal.Application.Features.MasterData.DTOs;

namespace BooksPortal.Application.Features.MasterData.Interfaces;

public interface IAcademicYearService
{
    Task<List<AcademicYearResponse>> GetAllAsync();
    Task<AcademicYearResponse?> GetActiveAsync();
    Task<AcademicYearResponse> GetByIdAsync(int id);
    Task<AcademicYearResponse> CreateAsync(CreateAcademicYearRequest request);
    Task<AcademicYearResponse> UpdateAsync(int id, UpdateAcademicYearRequest request);
    Task ActivateAsync(int id);
    Task DeleteAsync(int id);
}
