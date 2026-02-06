using BooksPortal.Application.Features.MasterData.DTOs;
using BooksPortal.Application.Features.MasterData.Interfaces;
using BooksPortal.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BooksPortal.API.Controllers;

[Authorize]
public class ClassSectionsController : ApiControllerBase
{
    private readonly IClassSectionService _service;

    public ClassSectionsController(IClassSectionService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? academicYearId)
        => OkResponse(await _service.GetAllAsync(academicYearId));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
        => OkResponse(await _service.GetByIdAsync(id));

    [HttpPost]
    public async Task<IActionResult> Create(CreateClassSectionRequest request)
        => CreatedResponse(await _service.CreateAsync(request));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CreateClassSectionRequest request)
        => OkResponse(await _service.UpdateAsync(id, request));

    [HttpDelete("{id}")]
    [Authorize(Roles = UserRole.SuperAdmin)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return OkResponse("Class section deleted.");
    }
}
