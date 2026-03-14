using Karve.Invoicing.Application.Exceptions;
using Karve.Invoicing.Application.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Karve.Invoicing.Api.Middleware;

/// <summary>
/// Middleware for handling exceptions and returning standardized error responses.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionHandlingMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The logger instance.</param>
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Handles exceptions thrown during request processing and returns a standardized error response.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred.");

            // If headers/body already started (for example during auth redirects),
            // we cannot safely rewrite the response as JSON.
            if (context.Response.HasStarted)
            {
                _logger.LogWarning(
                    "The response has already started, so the exception middleware will rethrow.");
                throw;
            }

            context.Response.ContentType = "application/json";

            var (statusCode, message) = ex switch
            {
                ForbiddenException => ((int)HttpStatusCode.Forbidden, ex.Message),
                _ => ((int)HttpStatusCode.InternalServerError, "An internal server error occurred. Please try again later.")
            };

            context.Response.StatusCode = statusCode;

            var response = ApiResponse<object>.Failure(message);

            var json = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(json);
        }
    }
}