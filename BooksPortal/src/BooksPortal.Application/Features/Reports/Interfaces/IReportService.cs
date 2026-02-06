using BooksPortal.Application.Features.Reports.DTOs;

namespace BooksPortal.Application.Features.Reports.Interfaces;

public interface IReportService
{
    Task<List<StockSummaryReport>> GetStockSummaryAsync(int? subjectId = null, string? grade = null);
    Task<List<DistributionSummaryReport>> GetDistributionSummaryAsync(int academicYearId, DateTime? from = null, DateTime? to = null);
    Task<List<TeacherOutstandingReport>> GetTeacherOutstandingAsync(int? teacherId = null);
    Task<List<StudentHistoryReport>> GetStudentHistoryAsync(int studentId);
    Task<byte[]> ExportStockSummaryAsync(int? subjectId = null, string? grade = null);
    Task<byte[]> ExportDistributionSummaryAsync(int academicYearId, DateTime? from = null, DateTime? to = null);
    Task<byte[]> ExportTeacherOutstandingAsync(int? teacherId = null);
}
