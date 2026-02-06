using BooksPortal.Application.Features.Reports.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BooksPortal.API.Controllers;

[Authorize]
public class ReportsController : ApiControllerBase
{
    private readonly IReportService _service;

    public ReportsController(IReportService service) => _service = service;

    [HttpGet("stock-summary")]
    public async Task<IActionResult> StockSummary(int? subjectId = null, string? grade = null)
        => OkResponse(await _service.GetStockSummaryAsync(subjectId, grade));

    [HttpGet("distribution-summary")]
    public async Task<IActionResult> DistributionSummary(int academicYearId, DateTime? from = null, DateTime? to = null)
        => OkResponse(await _service.GetDistributionSummaryAsync(academicYearId, from, to));

    [HttpGet("teacher-outstanding")]
    public async Task<IActionResult> TeacherOutstanding(int? teacherId = null)
        => OkResponse(await _service.GetTeacherOutstandingAsync(teacherId));

    [HttpGet("student-history/{studentId}")]
    public async Task<IActionResult> StudentHistory(int studentId)
        => OkResponse(await _service.GetStudentHistoryAsync(studentId));

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
}
