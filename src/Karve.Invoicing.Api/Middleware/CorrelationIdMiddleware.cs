using System.Diagnostics;

namespace Karve.Invoicing.Api.Middleware;

/// <summary>
/// Adds a request correlation identifier to the current request, response, and logging scope.
/// </summary>
public sealed class CorrelationIdMiddleware
{
    /// <summary>
    /// Standard response and request header name for correlation IDs.
    /// </summary>
    public const string HeaderName = "X-Correlation-ID";

    private const string ScopeCorrelationIdKey = "CorrelationId";
    private const string ScopeTraceIdKey = "TraceId";

    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CorrelationIdMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The logger instance.</param>
    public CorrelationIdMiddleware(
        RequestDelegate next,
        ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Applies correlation metadata to the request lifecycle.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = ResolveCorrelationId(context);
        context.TraceIdentifier = correlationId;

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        Activity.Current?.SetTag("correlation.id", correlationId);
        Activity.Current?.AddBaggage("correlation.id", correlationId);

        using var scope = _logger.BeginScope(new Dictionary<string, object?>
        {
            [ScopeCorrelationIdKey] = correlationId,
            [ScopeTraceIdKey] = Activity.Current?.TraceId.ToString()
        });

        await _next(context);
    }

    private static string ResolveCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(HeaderName, out var existingCorrelationId))
        {
            var value = existingCorrelationId.ToString().Trim();
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N");
    }
}