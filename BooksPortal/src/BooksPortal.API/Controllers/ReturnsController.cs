using BooksPortal.Application.Features.Returns.DTOs;
using BooksPortal.Application.Features.Returns.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BooksPortal.API.Controllers;

[Authorize]
public class ReturnsController : ApiControllerBase
{
    private readonly IReturnService _service;

    public ReturnsController(IReturnService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetPaged(int pageNumber = 1, int pageSize = 20, int? academicYearId = null, int? studentId = null)
        => OkResponse(await _service.GetPagedAsync(pageNumber, pageSize, academicYearId, studentId));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
        => OkResponse(await _service.GetByIdAsync(id));

    [HttpGet("by-reference/{referenceNo}")]
    public async Task<IActionResult> GetByReference(string referenceNo)
        => OkResponse(await _service.GetByReferenceAsync(referenceNo));

    [HttpPost]
    public async Task<IActionResult> Create(CreateReturnSlipRequest request)
        => CreatedResponse(await _service.CreateAsync(request, CurrentUserId));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Cancel(int id)
    {
        await _service.CancelAsync(id);
        return OkResponse("Return slip cancelled.");
    }
}
