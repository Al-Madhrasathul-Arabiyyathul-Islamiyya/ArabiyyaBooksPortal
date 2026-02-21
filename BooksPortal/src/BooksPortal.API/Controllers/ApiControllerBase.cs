using System.Security.Claims;
using BooksPortal.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace BooksPortal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    protected int CurrentUserId
        => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    protected string? CurrentUserName
        => User.FindFirstValue(ClaimTypes.Name);

    protected IActionResult OkResponse<T>(T data, string? message = null)
        => Ok(ApiResponse<T>.Ok(data, message));

    protected IActionResult CreatedResponse<T>(T data, string? message = null)
        => StatusCode(201, ApiResponse<T>.Ok(data, message));

    protected IActionResult FailResponse(string message, int statusCode = 400)
        => StatusCode(statusCode, ApiResponse<object>.Fail(message));
}
