using BooksPortal.Application.Features.BulkImport.Interfaces;
using BooksPortal.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BooksPortal.API.Controllers;

[Authorize(Roles = $"{UserRole.SuperAdmin},{UserRole.Admin}")]
[Route("api/import-templates")]
public class ImportTemplatesController : ApiControllerBase
{
    private readonly IImportTemplateService _service;

    public ImportTemplatesController(IImportTemplateService service)
    {
        _service = service;
    }

    [HttpGet("books")]
    public IActionResult GetBooksTemplate()
    {
        var bytes = _service.CreateBooksTemplate();
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "books-import-template.xlsx");
    }

    [HttpGet("teachers")]
    public IActionResult GetTeachersTemplate()
    {
        var bytes = _service.CreateTeachersTemplate();
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "teachers-import-template.xlsx");
    }

    [HttpGet("students")]
    public IActionResult GetStudentsTemplate()
    {
        var bytes = _service.CreateStudentsTemplate();
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "students-import-template.xlsx");
    }
}
