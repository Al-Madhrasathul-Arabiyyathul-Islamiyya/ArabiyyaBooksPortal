using BooksPortal.Application.Features.MasterData.DTOs;

namespace BooksPortal.Application.Features.MasterData.Interfaces;

public interface ILookupService
{
    Task<List<LookupResponse>> GetAcademicYearsAsync();
    Task<List<LookupResponse>> GetKeystagesAsync();
    Task<List<LookupResponse>> GetGradesAsync(int? keystageId = null);
    Task<List<LookupResponse>> GetSubjectsAsync();
    Task<List<LookupResponse>> GetClassSectionsAsync(int? academicYearId = null);
    Task<List<LookupResponse>> GetTermsAsync();
    Task<List<LookupResponse>> GetBookConditionsAsync();
    Task<List<LookupResponse>> GetMovementTypesAsync();
    Task<List<StudentOperationsLookupResponse>> GetStudentsForOperationsAsync(int academicYearId, string? search = null, int take = 20);
    Task<List<ParentLookupSummaryResponse>> GetParentsForOperationsAsync(int studentId, string? search = null, int take = 20);
    Task<List<BookOperationsLookupResponse>> GetBooksForOperationsAsync(int academicYearId, string? search = null, int take = 20);
    Task<List<TeacherOperationsLookupResponse>> GetTeachersForOperationsAsync(int? academicYearId = null, string? search = null, int take = 20);
    Task<List<TeacherIssueOutstandingLookupResponse>> GetTeacherIssueOutstandingForOperationsAsync(int? academicYearId = null, int? teacherId = null, string? search = null, int take = 20);
}
