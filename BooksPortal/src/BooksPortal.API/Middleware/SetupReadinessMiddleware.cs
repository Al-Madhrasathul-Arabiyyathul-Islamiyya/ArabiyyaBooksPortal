using BooksPortal.Application.Features.Setup.Interfaces;

namespace BooksPortal.API.Middleware;

public class SetupReadinessMiddleware
{
    private static readonly string[] GuardedPrefixes =
    [
        "/api/distributions",
        "/api/returns",
        "/api/teacherissues",
        "/api/teacherreturns"
    ];

    private readonly RequestDelegate _next;

    public SetupReadinessMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ISetupReadinessService setupReadinessService)
    {
        if (ShouldSkip(context))
        {
            await _next(context);
            return;
        }

        await setupReadinessService.EnsureReadyOrThrowAsync(context.RequestAborted);
        await _next(context);
    }

    private static bool ShouldSkip(HttpContext context)
    {
        if (HttpMethods.IsGet(context.Request.Method)
            || HttpMethods.IsHead(context.Request.Method)
            || HttpMethods.IsOptions(context.Request.Method))
            return true;

        var path = context.Request.Path.Value ?? string.Empty;
        if (path.StartsWith("/api/setup", StringComparison.OrdinalIgnoreCase))
            return true;

        return GuardedPrefixes.All(prefix => !path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }
}
