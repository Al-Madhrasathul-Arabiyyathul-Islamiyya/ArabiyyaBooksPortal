using BooksPortal.Application.Common.Interfaces;
using BooksPortal.Application.Features.Distribution.DTOs;
using BooksPortal.Application.Features.Distribution.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BooksPortal.API.Controllers;

[Authorize]
public class DistributionsController : ApiControllerBase
{
    private readonly IDistributionService _service;
    private readonly IPdfService _pdfService;

    public DistributionsController(IDistributionService service, IPdfService pdfService)
    {
        _service = service;
        _pdfService = pdfService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(int pageNumber = 1, int pageSize = 20, int? academicYearId = null, int? studentId = null)
        => OkResponse(await _service.GetPagedAsync(pageNumber, pageSize, academicYearId, studentId));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
        => OkResponse(await _service.GetByIdAsync(id));

    [HttpGet("by-reference/{referenceNo}")]
    public async Task<IActionResult> GetByReference(string referenceNo)
        => OkResponse(await _service.GetByReferenceAsync(referenceNo));

    [HttpPost]
    public async Task<IActionResult> Create(CreateDistributionSlipRequest request)
        => CreatedResponse(await _service.CreateAsync(request, CurrentUserId));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Cancel(int id)
    {
        await _service.CancelAsync(id);
        return OkResponse("Distribution slip cancelled.");
    }

    [HttpGet("{id}/print")]
    public async Task<IActionResult> Print(int id)
    {
        var slip = await _service.GetByIdAsync(id);
        var pdf = _pdfService.GenerateDistributionSlip(slip);
        return File(pdf, "application/pdf", $"Distribution-{slip.ReferenceNo}.pdf");
    }
}
