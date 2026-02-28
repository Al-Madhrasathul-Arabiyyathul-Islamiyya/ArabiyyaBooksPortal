using BooksPortal.Application.Features.Setup.Interfaces;
using BooksPortal.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BooksPortal.API.Controllers;

[Authorize]
public class SetupController : ApiControllerBase
{
    private readonly ISetupReadinessService _service;

    public SetupController(ISetupReadinessService service)
    {
        _service = service;
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetStatus(CancellationToken cancellationToken)
        => OkResponse(await _service.GetStatusAsync(cancellationToken));

    [HttpPost("start")]
    [Authorize(Roles = UserRole.SuperAdmin)]
    public async Task<IActionResult> Start(CancellationToken cancellationToken)
        => OkResponse(await _service.StartAsync(cancellationToken));

    [HttpPost("super-admin")]
    [Authorize(Roles = UserRole.SuperAdmin)]
    public async Task<IActionResult> ConfirmSuperAdmin(CancellationToken cancellationToken)
        => OkResponse(await _service.ConfirmSuperAdminAsync(cancellationToken));

    [HttpPost("slip-templates/confirm")]
    [Authorize(Roles = UserRole.SuperAdmin)]
    public async Task<IActionResult> ConfirmSlipTemplates(CancellationToken cancellationToken)
        => OkResponse(await _service.ConfirmSlipTemplatesAsync(cancellationToken));

    [HttpPost("hierarchy/initialize")]
    [Authorize(Roles = UserRole.SuperAdmin)]
    public async Task<IActionResult> InitializeHierarchy(CancellationToken cancellationToken)
        => OkResponse(await _service.InitializeHierarchyAsync(cancellationToken));

    [HttpPost("reference-formats/initialize")]
    [Authorize(Roles = UserRole.SuperAdmin)]
    public async Task<IActionResult> InitializeReferenceFormats(CancellationToken cancellationToken)
        => OkResponse(await _service.InitializeReferenceFormatsAsync(cancellationToken));

    [HttpPost("complete")]
    [Authorize(Roles = UserRole.SuperAdmin)]
    public async Task<IActionResult> Complete(CancellationToken cancellationToken)
        => OkResponse(await _service.CompleteAsync(cancellationToken));
}
