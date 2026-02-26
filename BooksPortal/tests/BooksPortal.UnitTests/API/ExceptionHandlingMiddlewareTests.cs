using System.Text;
using BooksPortal.API.Middleware;
using BooksPortal.Application.Common.Exceptions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BooksPortal.UnitTests.API;

public class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_BusinessRuleException_LogsHandledWarningWithoutExceptionObject()
    {
        var logger = new TestLogger<ExceptionHandlingMiddleware>();
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new BusinessRuleException("Invalid email or password."),
            logger);

        var context = new DefaultHttpContext();
        context.TraceIdentifier = "trace-handled";
        context.Request.Method = HttpMethods.Post;
        context.Request.Path = "/api/auth/login";
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        logger.Entries.Should().ContainSingle();
        var handledEntry = logger.Entries.Single();
        handledEntry.LogLevel.Should().Be(LogLevel.Warning);
        handledEntry.Exception.Should().BeNull();
        handledEntry.Message.Should().Contain("Handled exception");
        handledEntry.Message.Should().Contain("Invalid email or password");

        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body, Encoding.UTF8).ReadToEndAsync();
        body.Should().Contain("Invalid email or password.");
    }

    [Fact]
    public async Task InvokeAsync_UnhandledException_LogsErrorWithExceptionObject()
    {
        var logger = new TestLogger<ExceptionHandlingMiddleware>();
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new InvalidOperationException("boom"),
            logger);

        var context = new DefaultHttpContext();
        context.TraceIdentifier = "trace-unhandled";
        context.Request.Method = HttpMethods.Get;
        context.Request.Path = "/api/health";
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        logger.Entries.Should().ContainSingle();
        var unhandledEntry = logger.Entries.Single();
        unhandledEntry.LogLevel.Should().Be(LogLevel.Error);
        unhandledEntry.Exception.Should().BeOfType<InvalidOperationException>();
        unhandledEntry.Message.Should().Contain("Unhandled exception");

        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body, Encoding.UTF8).ReadToEndAsync();
        body.Should().Contain("An unexpected error occurred.");
    }

    private sealed class TestLogger<T> : ILogger<T>
    {
        public List<TestLogEntry> Entries { get; } = new();

        IDisposable? ILogger.BeginScope<TState>(TState state) => NullScope.Instance;

        bool ILogger.IsEnabled(LogLevel logLevel) => true;

        void ILogger.Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Entries.Add(new TestLogEntry(logLevel, formatter(state, exception), exception));
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose() { }
        }
    }

    private sealed record TestLogEntry(LogLevel LogLevel, string Message, Exception? Exception);
}
