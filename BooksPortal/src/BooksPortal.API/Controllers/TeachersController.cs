using BooksPortal.Application.Features.MasterData.DTOs;
using BooksPortal.Application.Features.MasterData.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BooksPortal.API.Controllers;

[Authorize]
public class TeachersController : ApiControllerBase
{
    private readonly ITeacherService _service;

    public TeachersController(ITeacherService service) => _service = service;

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
}
