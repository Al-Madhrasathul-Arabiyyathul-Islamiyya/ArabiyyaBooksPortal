using BooksPortal.Application.Common.Interfaces;
using BooksPortal.Application.Features.TeacherIssues.DTOs;
using BooksPortal.Application.Features.TeacherIssues.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BooksPortal.API.Controllers;

[Authorize]
public class TeacherIssuesController : ApiControllerBase
{
    private readonly ITeacherIssueService _service;
    private readonly IPdfService _pdfService;
    private readonly ISlipStorageService _storageService;

    public TeacherIssuesController(ITeacherIssueService service, IPdfService pdfService, ISlipStorageService storageService)
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
        bool includeCancelled = false)
        => OkResponse(await _service.GetPagedAsync(pageNumber, pageSize, academicYearId, teacherId, includeCancelled));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
        => OkResponse(await _service.GetByIdAsync(id));

    [HttpPost]
    public async Task<IActionResult> Create(CreateTeacherIssueRequest request)
        => CreatedResponse(await _service.CreateAsync(request, CurrentUserId));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateTeacherIssueRequest request)
        => OkResponse(await _service.UpdateAsync(id, request, CurrentUserId));

    [HttpPost("{id}/return")]
    public async Task<IActionResult> ProcessReturn(int id, ProcessTeacherReturnRequest request)
        => OkResponse(await _service.ProcessReturnAsync(id, request, CurrentUserId));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Cancel(int id)
    {
        await _service.CancelAsync(id, CurrentUserId);
        return OkResponse("Teacher issue cancelled.");
    }

    [HttpPost("{id}/finalize")]
    public async Task<IActionResult> FinalizeIssue(int id)
    {
        await _service.FinalizeAsync(id, CurrentUserId);
        return OkResponse("Teacher issue finalized.");
    }

    [HttpGet("{id}/print")]
    public async Task<IActionResult> Print(int id)
    {
        var issue = await _service.GetByIdAsync(id);

        var stored = await _storageService.LoadAsync(issue.PdfFilePath);
        if (stored != null)
            return File(stored, "application/pdf", $"TeacherIssue-{issue.ReferenceNo}-{issue.LifecycleStatus}.pdf");

        var pdf = await _pdfService.GenerateTeacherIssueSlipAsync(issue);
        return File(pdf, "application/pdf", $"TeacherIssue-{issue.ReferenceNo}-{issue.LifecycleStatus}.pdf");
    }

    [HttpGet("{id}/return/print")]
    public async Task<IActionResult> PrintLatestReturn(int id)
    {
        var slip = await _service.GetLatestReturnSlipByIssueIdAsync(id);

        var stored = await _storageService.LoadAsync(slip.PdfFilePath);
        if (stored != null)
            return File(stored, "application/pdf", $"TeacherReturn-{slip.ReferenceNo}-{slip.LifecycleStatus}.pdf");

        var pdf = await _pdfService.GenerateTeacherReturnSlipAsync(slip);
        return File(pdf, "application/pdf", $"TeacherReturn-{slip.ReferenceNo}-{slip.LifecycleStatus}.pdf");
    }
}
