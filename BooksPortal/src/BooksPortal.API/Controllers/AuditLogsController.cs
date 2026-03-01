using BooksPortal.Application.Features.AuditLogs.Interfaces;
using BooksPortal.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BooksPortal.API.Controllers;

[Authorize(Roles = $"{UserRole.SuperAdmin},{UserRole.Admin}")]
public class AuditLogsController : ApiControllerBase
{
    private readonly IAuditLogService _service;

    public AuditLogsController(IAuditLogService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        int pageNumber = 1, int pageSize = 20,
        string? entityType = null, string? action = null,
        int? userId = null, DateTime? from = null, DateTime? to = null)
        => OkResponse(await _service.GetPagedAsync(pageNumber, pageSize, entityType, action, userId, from, to));
}
