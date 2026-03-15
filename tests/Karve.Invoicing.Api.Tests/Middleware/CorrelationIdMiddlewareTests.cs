using Karve.Invoicing.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace Karve.Invoicing.Api.Tests.Middleware;

public class CorrelationIdMiddlewareTests
{
    private static CorrelationIdMiddleware Create(RequestDelegate next) =>
        new(next, NullLogger<CorrelationIdMiddleware>.Instance);

    [Fact]
    public async Task InvokeAsync_WithNoCorrelationIdHeader_SetsNonEmptyTraceIdentifier()
    {
        var context = new DefaultHttpContext();
        var middleware = Create(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        Assert.NotNull(context.TraceIdentifier);
        Assert.NotEmpty(context.TraceIdentifier);
    }

    [Fact]
    public async Task InvokeAsync_WithValidCorrelationIdHeader_UsesProvidedId()
    {
        const string correlationId = "test-correlation-abc-12345";
        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationIdMiddleware.HeaderName] = correlationId;
        var middleware = Create(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        Assert.Equal(correlationId, context.TraceIdentifier);
    }

    [Fact]
    public async Task InvokeAsync_WithWhitespaceOnlyHeader_GeneratesNewId()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationIdMiddleware.HeaderName] = "   ";
        var middleware = Create(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        Assert.False(string.IsNullOrWhiteSpace(context.TraceIdentifier));
        Assert.NotEqual("   ", context.TraceIdentifier);
    }

    [Fact]
    public async Task InvokeAsync_AlwaysCallsNextMiddleware()
    {
        var context = new DefaultHttpContext();
        var nextWasCalled = false;
        var middleware = Create(_ =>
        {
            nextWasCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        Assert.True(nextWasCalled);
    }

    [Fact]
    public async Task InvokeAsync_TwoRequestsWithNoHeader_GenerateDifferentIds()
    {
        var context1 = new DefaultHttpContext();
        var context2 = new DefaultHttpContext();
        var middleware = Create(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context1);
        await middleware.InvokeAsync(context2);

        Assert.NotEqual(context1.TraceIdentifier, context2.TraceIdentifier);
    }
}
