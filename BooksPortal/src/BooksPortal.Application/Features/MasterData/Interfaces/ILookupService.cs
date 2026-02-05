using BooksPortal.Application.Features.MasterData.DTOs;

namespace BooksPortal.Application.Features.MasterData.Interfaces;

public interface ILookupService
{
    Task<List<LookupResponse>> GetAcademicYearsAsync();
    Task<List<LookupResponse>> GetKeystagesAsync();
    Task<List<LookupResponse>> GetSubjectsAsync();
    Task<List<LookupResponse>> GetClassSectionsAsync(int? academicYearId = null);
    Task<List<LookupResponse>> GetTermsAsync();
    Task<List<LookupResponse>> GetBookConditionsAsync();
    Task<List<LookupResponse>> GetMovementTypesAsync();
}
