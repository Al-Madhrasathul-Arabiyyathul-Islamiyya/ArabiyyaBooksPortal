using BooksPortal.Application.Features.MasterData.DTOs;
using BooksPortal.Application.Features.MasterData.Interfaces;
using BooksPortal.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BooksPortal.API.Controllers;

[Authorize]
public class GradesController : ApiControllerBase
{
    private readonly IGradeService _service;

    public GradesController(IGradeService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? keystageId = null)
        => OkResponse(await _service.GetAllAsync(keystageId));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
        => OkResponse(await _service.GetByIdAsync(id));

    [HttpPost]
    [Authorize(Roles = UserRole.SuperAdmin)]
    public async Task<IActionResult> Create(CreateGradeRequest request)
        => CreatedResponse(await _service.CreateAsync(request));

    [HttpPut("{id}")]
    [Authorize(Roles = UserRole.SuperAdmin)]
    public async Task<IActionResult> Update(int id, CreateGradeRequest request)
        => OkResponse(await _service.UpdateAsync(id, request));

    [HttpDelete("{id}")]
    [Authorize(Roles = UserRole.SuperAdmin)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return OkResponse("Grade deleted.");
    }
}
