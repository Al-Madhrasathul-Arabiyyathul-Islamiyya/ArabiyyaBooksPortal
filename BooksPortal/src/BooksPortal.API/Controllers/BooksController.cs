using BooksPortal.Application.Features.Books.DTOs;
using BooksPortal.Application.Features.Books.Interfaces;
using BooksPortal.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BooksPortal.API.Controllers;

[Authorize]
public class BooksController : ApiControllerBase
{
    private readonly IBookService _service;

    public BooksController(IBookService service) => _service = service;

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
    public async Task<IActionResult> GetStockEntries(int id)
        => OkResponse(await _service.GetStockEntriesAsync(id));

    [HttpGet("{id}/stock-movements")]
    public async Task<IActionResult> GetStockMovements(int id)
        => OkResponse(await _service.GetStockMovementsAsync(id));

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
}
