using Karve.Invoicing.Application.Responses;
using Karve.Invoicing.Application.Services;

namespace Karve.Invoicing.Api.Middleware;

/// <summary>
/// Resolves tenant context from the authenticated user and enforces company membership.
/// </summary>
public sealed class TenantResolutionMiddleware
{
    /// <summary>
    /// HttpContext item key containing all company IDs available to the current user.
    /// </summary>
    public const string AvailableCompanyIdsItemKey = "AvailableCompanyIds";

    private readonly RequestDelegate _next;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantResolutionMiddleware"/> class.
    /// </summary>
    /// <param name="next">Next middleware in the request pipeline.</param>
    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Ensures authenticated users have at least one company and stores multi-company context.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="currentUser">Current user abstraction.</param>
    public async Task InvokeAsync(HttpContext context, ICurrentUserService currentUser)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var companyIds = currentUser.CompanyIds;

            if (companyIds.Count == 0)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(
                    ApiResponse<object>.Failure("User is not assigned to any company."));
                return;
            }

            if (companyIds.Count > 1)
            {
                // Store available tenants for future explicit tenant-selection flow.
                context.Items[AvailableCompanyIdsItemKey] = companyIds.ToList();
            }
        }

        await _next(context);
    }
}
