using BooksPortal.Application.Features.MasterData.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BooksPortal.API.Controllers;

[Authorize]
public class LookupsController : ApiControllerBase
{
    private readonly ILookupService _service;

    public LookupsController(ILookupService service) => _service = service;

    [HttpGet("academic-years")]
    public async Task<IActionResult> GetAcademicYears()
        => OkResponse(await _service.GetAcademicYearsAsync());

    [HttpGet("keystages")]
    public async Task<IActionResult> GetKeystages()
        => OkResponse(await _service.GetKeystagesAsync());

    [HttpGet("subjects")]
    public async Task<IActionResult> GetSubjects()
        => OkResponse(await _service.GetSubjectsAsync());

    [HttpGet("class-sections")]
    public async Task<IActionResult> GetClassSections([FromQuery] int? academicYearId)
        => OkResponse(await _service.GetClassSectionsAsync(academicYearId));

    [HttpGet("terms")]
    public async Task<IActionResult> GetTerms()
        => OkResponse(await _service.GetTermsAsync());

    [HttpGet("book-conditions")]
    public async Task<IActionResult> GetBookConditions()
        => OkResponse(await _service.GetBookConditionsAsync());

    [HttpGet("movement-types")]
    public async Task<IActionResult> GetMovementTypes()
        => OkResponse(await _service.GetMovementTypesAsync());
}
