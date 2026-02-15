using BooksPortal.Application.Features.Reports.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BooksPortal.API.Controllers;

[Authorize]
public class ReportsController : ApiControllerBase
{
    private const int MaxPageSize = 100;
    private readonly IReportService _service;

    public ReportsController(IReportService service) => _service = service;

    [HttpGet("stock-summary")]
    public async Task<IActionResult> StockSummary(
        int pageNumber = 1,
        int pageSize = 20,
        int? subjectId = null,
        string? grade = null)
        => OkResponse(await _service.GetStockSummaryAsync(NormalizePageNumber(pageNumber), NormalizePageSize(pageSize), subjectId, grade));

    [HttpGet("distribution-summary")]
    public async Task<IActionResult> DistributionSummary(
        int academicYearId,
        int pageNumber = 1,
        int pageSize = 20,
        DateTime? from = null,
        DateTime? to = null)
        => OkResponse(await _service.GetDistributionSummaryAsync(NormalizePageNumber(pageNumber), NormalizePageSize(pageSize), academicYearId, from, to));

    [HttpGet("teacher-outstanding")]
    public async Task<IActionResult> TeacherOutstanding(
        int pageNumber = 1,
        int pageSize = 20,
        int? teacherId = null)
        => OkResponse(await _service.GetTeacherOutstandingAsync(NormalizePageNumber(pageNumber), NormalizePageSize(pageSize), teacherId));

    [HttpGet("student-history/{studentId}")]
    public async Task<IActionResult> StudentHistory(int studentId, int pageNumber = 1, int pageSize = 20)
        => OkResponse(await _service.GetStudentHistoryAsync(NormalizePageNumber(pageNumber), NormalizePageSize(pageSize), studentId));

    [HttpGet("export/stock-summary")]
    public async Task<IActionResult> ExportStockSummary(int? subjectId = null, string? grade = null)
    {
        var bytes = await _service.ExportStockSummaryAsync(subjectId, grade);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "stock-summary.xlsx");
    }

    [HttpGet("export/distribution-summary")]
    public async Task<IActionResult> ExportDistributionSummary(int academicYearId, DateTime? from = null, DateTime? to = null)
    {
        var bytes = await _service.ExportDistributionSummaryAsync(academicYearId, from, to);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "distribution-summary.xlsx");
    }

    [HttpGet("export/teacher-outstanding")]
    public async Task<IActionResult> ExportTeacherOutstanding(int? teacherId = null)
    {
        var bytes = await _service.ExportTeacherOutstandingAsync(teacherId);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "teacher-outstanding.xlsx");
    }

    private static int NormalizePageSize(int pageSize)
        => pageSize <= 0 ? 20 : Math.Min(pageSize, MaxPageSize);

    private static int NormalizePageNumber(int pageNumber)
        => pageNumber <= 0 ? 1 : pageNumber;
}
