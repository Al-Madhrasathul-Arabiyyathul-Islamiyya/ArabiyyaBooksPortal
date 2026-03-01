using BooksPortal.Application.Common.Models;
using BooksPortal.Application.Features.Reports.DTOs;

namespace BooksPortal.Application.Features.Reports.Interfaces;

public interface IReportService
{
    Task<PaginatedList<StockSummaryReport>> GetStockSummaryAsync(
        int pageNumber,
        int pageSize,
        int? subjectId = null,
        string? grade = null);
    Task<PaginatedList<DistributionSummaryReport>> GetDistributionSummaryAsync(
        int pageNumber,
        int pageSize,
        int academicYearId,
        DateTime? from = null,
        DateTime? to = null);
    Task<PaginatedList<TeacherOutstandingReport>> GetTeacherOutstandingAsync(
        int pageNumber,
        int pageSize,
        int? teacherId = null);
    Task<PaginatedList<StudentHistoryReport>> GetStudentHistoryAsync(int pageNumber, int pageSize, int studentId);
    Task<byte[]> ExportStockSummaryAsync(int? subjectId = null, string? grade = null);
    Task<byte[]> ExportDistributionSummaryAsync(int academicYearId, DateTime? from = null, DateTime? to = null);
    Task<byte[]> ExportTeacherOutstandingAsync(int? teacherId = null);
}
