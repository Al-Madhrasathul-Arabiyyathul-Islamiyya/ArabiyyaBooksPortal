using BooksPortal.Application.Features.Books.DTOs;
using BooksPortal.Application.Features.Books.Interfaces;
using BooksPortal.Application.Features.BulkImport.Interfaces;
using BooksPortal.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BooksPortal.API.Controllers;

[Authorize]
public class BooksController : ApiControllerBase
{
    private const int MaxPageSize = 100;
    private readonly IBookService _service;
    private readonly IBookBulkImportService _bulkImportService;

    public BooksController(IBookService service, IBookBulkImportService bulkImportService)
    {
        _service = service;
        _bulkImportService = bulkImportService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(int pageNumber = 1, int pageSize = 20, int? subjectId = null, string? search = null)
        => OkResponse(await _service.GetPagedAsync(pageNumber, pageSize, subjectId, search));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
        => OkResponse(await _service.GetByIdAsync(id));

    [HttpPost]
    public async Task<IActionResult> Create(CreateBookRequest request)
        => CreatedResponse(await _service.CreateAsync(request));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CreateBookRequest request)
        => OkResponse(await _service.UpdateAsync(id, request));

    [HttpDelete("{id}")]
    [Authorize(Roles = UserRole.SuperAdmin)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return OkResponse("Book deleted.");
    }

    [HttpPost("{id}/stock-entry")]
    public async Task<IActionResult> AddStock(int id, AddStockRequest request)
        => CreatedResponse(await _service.AddStockAsync(id, request, CurrentUserId));

    [HttpGet("{id}/stock-entries")]
    public async Task<IActionResult> GetStockEntries(int id, int pageNumber = 1, int pageSize = 20)
        => OkResponse(await _service.GetStockEntriesAsync(id, NormalizePageNumber(pageNumber), NormalizePageSize(pageSize)));

    [HttpGet("{id}/stock-movements")]
    public async Task<IActionResult> GetStockMovements(int id, int pageNumber = 1, int pageSize = 20)
        => OkResponse(await _service.GetStockMovementsAsync(id, NormalizePageNumber(pageNumber), NormalizePageSize(pageSize)));

    [HttpPost("{id}/adjust-stock")]
    [Authorize(Roles = $"{UserRole.SuperAdmin},{UserRole.Admin}")]
    public async Task<IActionResult> AdjustStock(int id, AdjustStockRequest request)
    {
        await _service.AdjustStockAsync(id, request, CurrentUserId);
        return OkResponse("Stock adjusted.");
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q)
        => OkResponse(await _service.SearchAsync(q));

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
        var report = await _bulkImportService.CommitAsync(stream, CurrentUserId, cancellationToken);
        return OkResponse(report);
    }

    private static int NormalizePageSize(int pageSize)
        => pageSize <= 0 ? 20 : Math.Min(pageSize, MaxPageSize);

    private static int NormalizePageNumber(int pageNumber)
        => pageNumber <= 0 ? 1 : pageNumber;
}
