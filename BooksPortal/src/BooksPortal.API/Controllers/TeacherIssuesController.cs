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

    public TeacherIssuesController(ITeacherIssueService service, IPdfService pdfService)
    {
        _service = service;
        _pdfService = pdfService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(int pageNumber = 1, int pageSize = 20, int? academicYearId = null, int? teacherId = null)
        => OkResponse(await _service.GetPagedAsync(pageNumber, pageSize, academicYearId, teacherId));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
        => OkResponse(await _service.GetByIdAsync(id));

    [HttpPost]
    public async Task<IActionResult> Create(CreateTeacherIssueRequest request)
        => CreatedResponse(await _service.CreateAsync(request, CurrentUserId));

    [HttpPost("{id}/return")]
    public async Task<IActionResult> ProcessReturn(int id, ProcessTeacherReturnRequest request)
        => OkResponse(await _service.ProcessReturnAsync(id, request, CurrentUserId));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Cancel(int id)
    {
        await _service.CancelAsync(id);
        return OkResponse("Teacher issue cancelled.");
    }

    [HttpGet("{id}/print")]
    public async Task<IActionResult> Print(int id)
    {
        var issue = await _service.GetByIdAsync(id);
        var pdf = await _pdfService.GenerateTeacherIssueSlipAsync(issue);
        return File(pdf, "application/pdf", $"TeacherIssue-{issue.ReferenceNo}.pdf");
    }
}
