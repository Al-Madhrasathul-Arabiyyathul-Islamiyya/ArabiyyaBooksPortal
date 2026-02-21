using BooksPortal.Application.Features.Settings.DTOs;
using BooksPortal.Application.Features.Settings.Interfaces;
using BooksPortal.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BooksPortal.API.Controllers;

[Authorize(Roles = $"{UserRole.SuperAdmin},{UserRole.Admin}")]
public class ReferenceNumberFormatsController : ApiControllerBase
{
    private readonly IReferenceNumberFormatService _service;

    public ReferenceNumberFormatsController(IReferenceNumberFormatService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] SlipType? slipType, [FromQuery] int? academicYearId)
        => OkResponse(await _service.GetAllAsync(slipType, academicYearId));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
        => OkResponse(await _service.GetByIdAsync(id));

    [HttpPost]
    public async Task<IActionResult> Create(CreateReferenceNumberFormatRequest request)
        => CreatedResponse(await _service.CreateAsync(request));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CreateReferenceNumberFormatRequest request)
        => OkResponse(await _service.UpdateAsync(id, request));

    [HttpDelete("{id}")]
    [Authorize(Roles = UserRole.SuperAdmin)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return OkResponse("Reference number format deleted.");
    }
}
