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

    [HttpGet("grades")]
    public async Task<IActionResult> GetGrades([FromQuery] int? keystageId)
        => OkResponse(await _service.GetGradesAsync(keystageId));

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

    [HttpGet("operations/students")]
    public async Task<IActionResult> GetStudentsForOperations([FromQuery] int academicYearId, [FromQuery] string? search = null, [FromQuery] int take = 20)
        => OkResponse(await _service.GetStudentsForOperationsAsync(academicYearId, search, take));

    [HttpGet("operations/parents")]
    public async Task<IActionResult> GetParentsForOperations([FromQuery] int studentId, [FromQuery] string? search = null, [FromQuery] int take = 20)
        => OkResponse(await _service.GetParentsForOperationsAsync(studentId, search, take));

    [HttpGet("operations/books")]
    public async Task<IActionResult> GetBooksForOperations([FromQuery] int academicYearId, [FromQuery] string? search = null, [FromQuery] int take = 20)
        => OkResponse(await _service.GetBooksForOperationsAsync(academicYearId, search, take));

    [HttpGet("operations/teachers")]
    public async Task<IActionResult> GetTeachersForOperations([FromQuery] int? academicYearId = null, [FromQuery] string? search = null, [FromQuery] int take = 20)
        => OkResponse(await _service.GetTeachersForOperationsAsync(academicYearId, search, take));

    [HttpGet("operations/teacher-issues/outstanding")]
    public async Task<IActionResult> GetTeacherIssueOutstandingForOperations(
        [FromQuery] int? academicYearId = null,
        [FromQuery] int? teacherId = null,
        [FromQuery] string? search = null,
        [FromQuery] int take = 20)
        => OkResponse(await _service.GetTeacherIssueOutstandingForOperationsAsync(academicYearId, teacherId, search, take));
}
