using BooksPortal.Application.Features.MasterData.DTOs;
using BooksPortal.Application.Features.MasterData.Interfaces;
using BooksPortal.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BooksPortal.API.Controllers;

[Authorize]
public class ClassSectionsController : ApiControllerBase
{
    private const int MaxPageSize = 100;
    private readonly IClassSectionService _service;

    public ClassSectionsController(IClassSectionService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] int? academicYearId = null,
        [FromQuery] int? keystageId = null,
        [FromQuery] int? gradeId = null,
        [FromQuery] string? search = null)
        => OkResponse(await _service.GetPagedAsync(NormalizePageNumber(pageNumber), NormalizePageSize(pageSize), academicYearId, keystageId, gradeId, search));

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

    private static int NormalizePageSize(int pageSize)
        => pageSize <= 0 ? 20 : Math.Min(pageSize, MaxPageSize);

    private static int NormalizePageNumber(int pageNumber)
        => pageNumber <= 0 ? 1 : pageNumber;
}
