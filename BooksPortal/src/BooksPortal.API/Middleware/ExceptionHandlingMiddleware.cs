using System.Text.Json;
using BooksPortal.Application.Common.Exceptions;
using BooksPortal.Application.Common.Models;

namespace BooksPortal.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            BadRequestException => (StatusCodes.Status400BadRequest, exception.Message),
            NotFoundException => (StatusCodes.Status404NotFound, exception.Message),
            BusinessRuleException => (StatusCodes.Status409Conflict, exception.Message),
            ForbiddenException => (StatusCodes.Status403Forbidden, exception.Message),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                exception,
                "Unhandled exception. {Method} {Path} responded {StatusCode}. TraceId: {TraceId}",
                context.Request.Method,
                context.Request.Path.Value,
                statusCode,
                context.TraceIdentifier);
        }
        else
        {
            _logger.LogWarning(
                "Handled exception: {Message}. {Method} {Path} responded {StatusCode}. TraceId: {TraceId}. ExceptionType: {ExceptionType}",
                exception.Message,
                context.Request.Method,
                context.Request.Path.Value,
                statusCode,
                context.TraceIdentifier,
                exception.GetType().Name);
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = ApiResponse<object>.Fail(message);
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
