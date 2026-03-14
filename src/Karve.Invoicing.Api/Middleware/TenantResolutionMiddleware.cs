using Karve.Invoicing.Application.Responses;
using Karve.Invoicing.Application.Services;

namespace Karve.Invoicing.Api.Middleware;

/// <summary>
/// Resolves tenant context from the authenticated user and enforces company membership.
/// </summary>
public sealed class TenantResolutionMiddleware
{
    /// <summary>
    /// HttpContext item key containing resolved company IDs for the current user.
    /// </summary>
    public const string ResolvedCompanyIdsItemKey = "ResolvedCompanyIds";

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
    /// <param name="companyMembershipService">Service for querying user company memberships.</param>
    public async Task InvokeAsync(HttpContext context, ICompanyMembershipService companyMembershipService)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            if (!context.Items.TryGetValue(UserProvisioningMiddleware.LocalUserIdItemKey, out var localUserIdObj) || localUserIdObj is not Guid localUserId)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(
                    ApiResponse<object>.Failure("Authenticated user provisioning context is missing."));
                return;
            }

            var companyIds = await companyMembershipService.GetCompanyIdsForUserAsync(localUserId);
            context.Items[ResolvedCompanyIdsItemKey] = companyIds;

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
