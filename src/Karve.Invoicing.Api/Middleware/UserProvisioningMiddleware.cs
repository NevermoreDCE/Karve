using System.Security.Claims;
using Karve.Invoicing.Application.Responses;
using Karve.Invoicing.Application.Services;

namespace Karve.Invoicing.Api.Middleware;

/// <summary>
/// Provisions local users for authenticated requests and stores the local user ID in the request context.
/// </summary>
public sealed class UserProvisioningMiddleware
{
    /// <summary>
    /// HttpContext item key containing the local provisioned user ID.
    /// </summary>
    public const string LocalUserIdItemKey = "LocalUserId";

    private readonly RequestDelegate _next;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserProvisioningMiddleware"/> class.
    /// </summary>
    /// <param name="next">Next middleware in pipeline.</param>
    public UserProvisioningMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Executes middleware logic for provisioning authenticated users.
    /// </summary>
    /// <param name="context">Current HTTP request context.</param>
    /// <param name="userProvisioningService">Provisioning service.</param>
    public async Task InvokeAsync(HttpContext context, IUserProvisioningService userProvisioningService)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var externalUserId = GetClaim(context.User, "oid")
                ?? GetClaim(context.User, ClaimTypes.NameIdentifier)
                ?? GetClaim(context.User, "sub");

            var email = GetClaim(context.User, "preferred_username")
                ?? GetClaim(context.User, "email")
                ?? GetClaim(context.User, ClaimTypes.Email);

            if (string.IsNullOrWhiteSpace(externalUserId) || string.IsNullOrWhiteSpace(email))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(
                    ApiResponse<object>.Failure("Authenticated token is missing required user claims."));
                return;
            }

            var localUserId = await userProvisioningService.EnsureUserProvisionedAsync(externalUserId, email);
            context.Items[LocalUserIdItemKey] = localUserId;
        }

        await _next(context);
    }

    private static string? GetClaim(ClaimsPrincipal user, string claimType)
    {
        return user.FindFirst(claimType)?.Value;
    }
}
