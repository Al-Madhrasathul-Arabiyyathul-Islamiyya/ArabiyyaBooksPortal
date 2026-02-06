using BooksPortal.Application.Features.Settings.DTOs;
using BooksPortal.Application.Features.Settings.Interfaces;
using BooksPortal.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BooksPortal.API.Controllers;

[Authorize(Roles = $"{UserRole.SuperAdmin},{UserRole.Admin}")]
public class SlipTemplateSettingsController : ApiControllerBase
{
    private readonly ISlipTemplateSettingService _service;

    public SlipTemplateSettingsController(ISlipTemplateSettingService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? category)
        => OkResponse(await _service.GetAllAsync(category));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
        => OkResponse(await _service.GetByIdAsync(id));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateSlipTemplateSettingRequest request)
        => OkResponse(await _service.UpdateAsync(id, request));

    [HttpPost("reset")]
    [Authorize(Roles = UserRole.SuperAdmin)]
    public async Task<IActionResult> ResetToDefaults()
    {
        await _service.ResetToDefaultsAsync();
        return OkResponse("Slip template settings reset to defaults.");
    }
}
