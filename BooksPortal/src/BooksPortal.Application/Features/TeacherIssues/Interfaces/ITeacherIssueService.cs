using BooksPortal.Application.Common.Models;
using BooksPortal.Application.Features.TeacherIssues.DTOs;

namespace BooksPortal.Application.Features.TeacherIssues.Interfaces;

public interface ITeacherIssueService
{
    Task<PaginatedList<TeacherIssueResponse>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        int? academicYearId = null,
        int? teacherId = null,
        bool includeCancelled = false);
    Task<TeacherIssueResponse> GetByIdAsync(int id);
    Task<TeacherIssueResponse> CreateAsync(CreateTeacherIssueRequest request, int userId);
    Task<TeacherIssueResponse> UpdateAsync(int id, UpdateTeacherIssueRequest request, int userId);
    Task<TeacherIssueResponse> ProcessReturnAsync(int id, ProcessTeacherReturnRequest request, int userId);
    Task<TeacherReturnSlipResponse> GetLatestReturnSlipByIssueIdAsync(int issueId);
    Task FinalizeAsync(int id, int userId);
    Task CancelAsync(int id, int userId);
}
