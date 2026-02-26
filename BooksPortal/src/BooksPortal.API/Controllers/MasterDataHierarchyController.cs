using BooksPortal.Application.Features.MasterData.DTOs;
using BooksPortal.Application.Features.MasterData.Interfaces;
using BooksPortal.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BooksPortal.API.Controllers;

[Authorize(Roles = UserRole.SuperAdmin)]
[Route("api/master-data/hierarchy")]
public class MasterDataHierarchyController : ApiControllerBase
{
    private readonly IMasterDataHierarchyBulkService _service;

    public MasterDataHierarchyController(IMasterDataHierarchyBulkService service)
    {
        _service = service;
    }

    [HttpPost("bulk/upsert")]
    public async Task<IActionResult> Upsert([FromBody] HierarchyBulkUpsertRequest request)
        => OkResponse(await _service.UpsertAsync(request));
}
