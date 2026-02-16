using BooksPortal.Application.Features.TeacherIssues.Interfaces;
using BooksPortal.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BooksPortal.API.Controllers;

[Authorize]
public class TeacherReturnsController : ApiControllerBase
{
    private readonly ITeacherIssueService _service;
    private readonly IPdfService _pdfService;
    private readonly ISlipStorageService _storageService;

    public TeacherReturnsController(
        ITeacherIssueService service,
        IPdfService pdfService,
        ISlipStorageService storageService)
    {
        _service = service;
        _pdfService = pdfService;
        _storageService = storageService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        int pageNumber = 1,
        int pageSize = 20,
        int? academicYearId = null,
        int? teacherId = null,
        int? teacherIssueId = null,
        bool includeCancelled = false)
        => OkResponse(await _service.GetTeacherReturnsPagedAsync(
            pageNumber,
            pageSize,
            academicYearId,
            teacherId,
            teacherIssueId,
            includeCancelled));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
        => OkResponse(await _service.GetTeacherReturnByIdAsync(id));

    [HttpGet("by-reference/{referenceNo}")]
    public async Task<IActionResult> GetByReference(string referenceNo)
        => OkResponse(await _service.GetTeacherReturnByReferenceAsync(referenceNo));

    [HttpPost("{id:int}/finalize")]
    public async Task<IActionResult> FinalizeSlip(int id)
    {
        await _service.FinalizeTeacherReturnAsync(id, CurrentUserId);
        return OkResponse("Teacher return slip finalized.");
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Cancel(int id)
    {
        await _service.CancelTeacherReturnAsync(id, CurrentUserId);
        return OkResponse("Teacher return slip cancelled.");
    }

    [HttpGet("{id:int}/print")]
    public async Task<IActionResult> Print(int id)
    {
        var slip = await _service.GetTeacherReturnByIdAsync(id);

        var stored = await _storageService.LoadAsync(slip.PdfFilePath);
        if (stored != null)
            return File(stored, "application/pdf", $"TeacherReturn-{slip.ReferenceNo}-{slip.LifecycleStatus}.pdf");

        var pdf = await _pdfService.GenerateTeacherReturnSlipAsync(slip);
        return File(pdf, "application/pdf", $"TeacherReturn-{slip.ReferenceNo}-{slip.LifecycleStatus}.pdf");
    }
}
