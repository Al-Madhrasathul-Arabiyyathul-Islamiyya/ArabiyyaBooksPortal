using BooksPortal.Application.Common.Interfaces;
using BooksPortal.Application.Features.Users.DTOs;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BooksPortal.API.Controllers;

[Route("api/users")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class UsersController : ApiControllerBase
{
    private const int MaxPageSize = 100;
    private readonly IUserService _userService;
    private readonly IValidator<CreateUserRequest> _createValidator;
    private readonly IValidator<UpdateUserRequest> _updateValidator;

    public UsersController(
        IUserService userService,
        IValidator<CreateUserRequest> createValidator,
        IValidator<UpdateUserRequest> updateValidator)
    {
        _userService = userService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? role = null)
    {
        var result = await _userService.GetPagedUsersAsync(NormalizePageNumber(pageNumber), NormalizePageSize(pageSize), search, isActive, role);
        return OkResponse(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _userService.GetUserByIdAsync(id);
        return OkResponse(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        var validation = await _createValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return FailResponse(validation.Errors.First().ErrorMessage);

        var result = await _userService.CreateUserAsync(request);
        return CreatedResponse(result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest request)
    {
        var validation = await _updateValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return FailResponse(validation.Errors.First().ErrorMessage);

        var result = await _userService.UpdateUserAsync(id, request);
        return OkResponse(result);
    }

    [HttpPost("{id:int}/toggle-active")]
    public async Task<IActionResult> ToggleActive(int id)
    {
        await _userService.ToggleUserActiveAsync(id);
        return OkResponse(true, "User active status toggled.");
    }

    [HttpPut("{id:int}/roles")]
    public async Task<IActionResult> AssignRoles(int id, [FromBody] List<string> roles)
    {
        await _userService.AssignRolesAsync(id, roles);
        return OkResponse(true, "Roles assigned successfully.");
    }

    private static int NormalizePageSize(int pageSize)
        => pageSize <= 0 ? 20 : Math.Min(pageSize, MaxPageSize);

    private static int NormalizePageNumber(int pageNumber)
        => pageNumber <= 0 ? 1 : pageNumber;
}
