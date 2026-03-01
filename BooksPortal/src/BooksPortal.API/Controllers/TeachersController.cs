using BooksPortal.Application.Features.MasterData.DTOs;
using BooksPortal.Application.Features.MasterData.Interfaces;
using BooksPortal.Application.Features.BulkImport.Interfaces;
using BooksPortal.API.Services;
using BooksPortal.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BooksPortal.API.Controllers;

[Authorize]
public class TeachersController : ApiControllerBase
{
    private readonly ITeacherService _service;
    private readonly ITeacherBulkImportService _bulkImportService;
    private readonly BookBulkImportJobStore _bulkImportJobStore;

    public TeachersController(
        ITeacherService service,
        ITeacherBulkImportService bulkImportService,
        BookBulkImportJobStore bulkImportJobStore)
    {
        _service = service;
        _bulkImportService = bulkImportService;
        _bulkImportJobStore = bulkImportJobStore;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null)
        => OkResponse(await _service.GetPagedAsync(pageNumber, pageSize, search));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
        => OkResponse(await _service.GetByIdAsync(id));

    [HttpPost]
    public async Task<IActionResult> Create(CreateTeacherRequest request)
        => CreatedResponse(await _service.CreateAsync(request));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CreateTeacherRequest request)
        => OkResponse(await _service.UpdateAsync(id, request));

    [HttpDelete("{id}")]
    [Authorize(Roles = UserRole.SuperAdmin)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return OkResponse("Teacher deleted.");
    }

    [HttpPost("{id}/assignments")]
    public async Task<IActionResult> AddAssignment(int id, CreateTeacherAssignmentRequest request)
        => CreatedResponse(await _service.AddAssignmentAsync(id, request));

    [HttpDelete("{id}/assignments/{assignmentId}")]
    public async Task<IActionResult> RemoveAssignment(int id, int assignmentId)
    {
        await _service.RemoveAssignmentAsync(id, assignmentId);
        return OkResponse("Assignment removed.");
    }

    [HttpPost("bulk/validate")]
    [Authorize(Roles = $"{UserRole.SuperAdmin},{UserRole.Admin}")]
    public async Task<IActionResult> ValidateBulk([FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length == 0)
            return FailResponse("File is required.");

        await using var stream = file.OpenReadStream();
        var report = await _bulkImportService.ValidateAsync(stream, cancellationToken);
        return OkResponse(report);
    }

    [HttpPost("bulk/commit")]
    [Authorize(Roles = $"{UserRole.SuperAdmin},{UserRole.Admin}")]
    public async Task<IActionResult> CommitBulk([FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length == 0)
            return FailResponse("File is required.");

        await using var stream = file.OpenReadStream();
        var report = await _bulkImportService.CommitAsync(stream, cancellationToken: cancellationToken);
        return OkResponse(report);
    }

    [HttpPost("bulk/commit-async")]
    [Authorize(Roles = $"{UserRole.SuperAdmin},{UserRole.Admin}")]
    public async Task<IActionResult> CommitBulkAsync([FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length == 0)
            return FailResponse("File is required.");

        await using var stream = file.OpenReadStream();
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory, cancellationToken);
        var jobId = _bulkImportJobStore.StartTeachers(memory.ToArray());
        return OkResponse(new { jobId }, "Bulk import job started.");
    }

    [HttpGet("bulk/jobs/{jobId:guid}")]
    [Authorize(Roles = $"{UserRole.SuperAdmin},{UserRole.Admin}")]
    public IActionResult GetBulkJobStatus(Guid jobId)
    {
        var snapshot = _bulkImportJobStore.Get(jobId);
        return snapshot is null
            ? FailResponse("Bulk import job not found.", 404)
            : OkResponse(snapshot);
    }

    [HttpGet("bulk/jobs/{jobId:guid}/report")]
    [Authorize(Roles = $"{UserRole.SuperAdmin},{UserRole.Admin}")]
    public IActionResult DownloadBulkJobReport(Guid jobId)
    {
        var report = _bulkImportJobStore.GetReport(jobId);
        return report is null
            ? FailResponse("Bulk import report is not ready.", 404)
            : File(report.Value.Bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", report.Value.FileName);
    }
}
