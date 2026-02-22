using BooksPortal.Application.Features.MasterData.DTOs;
using BooksPortal.Application.Features.MasterData.Interfaces;
using BooksPortal.Application.Features.BulkImport.Interfaces;
using BooksPortal.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BooksPortal.API.Controllers;

[Authorize]
public class ParentsController : ApiControllerBase
{
    private readonly IParentService _service;
    private readonly IParentBulkImportService _bulkImportService;

    public ParentsController(IParentService service, IParentBulkImportService bulkImportService)
    {
        _service = service;
        _bulkImportService = bulkImportService;
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
    public async Task<IActionResult> Create(CreateParentRequest request)
        => CreatedResponse(await _service.CreateAsync(request));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CreateParentRequest request)
        => OkResponse(await _service.UpdateAsync(id, request));

    [HttpDelete("{id}")]
    [Authorize(Roles = UserRole.SuperAdmin)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return OkResponse("Parent deleted.");
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
