using BooksPortal.API.Services;
using BooksPortal.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BooksPortal.API.Controllers;

[Authorize(Roles = $"{UserRole.SuperAdmin},{UserRole.Admin}")]
[Route("api/import-templates")]
public class ImportTemplatesController : ApiControllerBase
{
    private readonly ImportTemplateCacheService _cacheService;

    public ImportTemplatesController(ImportTemplateCacheService cacheService)
    {
        _cacheService = cacheService;
    }

    [HttpGet("books")]
    public async Task<IActionResult> GetBooksTemplate(CancellationToken cancellationToken)
    {
        var template = await _cacheService.GetTemplateAsync("books", cancellationToken);
        return template is null
            ? FailResponse("Books template is unavailable.")
            : File(template.Value.Bytes, template.Value.ContentType, template.Value.FileName);
    }

    [HttpGet("teachers")]
    public async Task<IActionResult> GetTeachersTemplate(CancellationToken cancellationToken)
    {
        var template = await _cacheService.GetTemplateAsync("teachers", cancellationToken);
        return template is null
            ? FailResponse("Teachers template is unavailable.")
            : File(template.Value.Bytes, template.Value.ContentType, template.Value.FileName);
    }

    [HttpGet("students")]
    public async Task<IActionResult> GetStudentsTemplate(CancellationToken cancellationToken)
    {
        var template = await _cacheService.GetTemplateAsync("students", cancellationToken);
        return template is null
            ? FailResponse("Students template is unavailable.")
            : File(template.Value.Bytes, template.Value.ContentType, template.Value.FileName);
    }

    [HttpGet("parents")]
    public async Task<IActionResult> GetParentsTemplate(CancellationToken cancellationToken)
    {
        var template = await _cacheService.GetTemplateAsync("parents", cancellationToken);
        return template is null
            ? FailResponse("Parents template is unavailable.")
            : File(template.Value.Bytes, template.Value.ContentType, template.Value.FileName);
    }

    [HttpGet("master-data-hierarchy")]
    [Authorize(Roles = UserRole.SuperAdmin)]
    public async Task<IActionResult> GetMasterDataHierarchyTemplate(CancellationToken cancellationToken)
    {
        var template = await _cacheService.GetTemplateAsync("master-data-hierarchy", cancellationToken);
        return template is null
            ? FailResponse("Master data hierarchy template is unavailable.")
            : File(template.Value.Bytes, template.Value.ContentType, template.Value.FileName);
    }
}
