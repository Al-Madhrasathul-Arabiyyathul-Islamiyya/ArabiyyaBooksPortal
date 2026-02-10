using BooksPortal.Application.Features.MasterData.DTOs;
using BooksPortal.Application.Features.MasterData.Interfaces;
using BooksPortal.Application.Features.BulkImport.Interfaces;
using BooksPortal.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BooksPortal.API.Controllers;

[Authorize]
public class StudentsController : ApiControllerBase
{
    private readonly IStudentService _service;
    private readonly IStudentBulkImportService _bulkImportService;

    public StudentsController(IStudentService service, IStudentBulkImportService bulkImportService)
    {
        _service = service;
        _bulkImportService = bulkImportService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] int? classSectionId = null,
        [FromQuery] string? search = null)
        => OkResponse(await _service.GetPagedAsync(pageNumber, pageSize, classSectionId, search));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
        => OkResponse(await _service.GetByIdAsync(id));

    [HttpPost]
    public async Task<IActionResult> Create(CreateStudentRequest request)
        => CreatedResponse(await _service.CreateAsync(request));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateStudentRequest request)
        => OkResponse(await _service.UpdateAsync(id, request));

    [HttpDelete("{id}")]
    [Authorize(Roles = UserRole.SuperAdmin)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return OkResponse("Student deleted.");
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
        var report = await _bulkImportService.CommitAsync(stream, cancellationToken);
        return OkResponse(report);
    }
}
