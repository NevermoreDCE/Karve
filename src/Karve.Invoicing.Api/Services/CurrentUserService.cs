using System.Security.Claims;
using Karve.Invoicing.Api.Middleware;
using Karve.Invoicing.Application.Services;

namespace Karve.Invoicing.Api.Services;

/// <summary>
/// Reads the current authenticated user from the HTTP context and exposes key claim values.
/// </summary>
public sealed class CurrentUserService : ICurrentUserService
{
    private static readonly IReadOnlyList<Guid> EmptyCompanyIds = Array.Empty<Guid>();
    private readonly ICompanyMembershipService _companyMembershipService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="CurrentUserService"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">Accessor for the active HTTP context.</param>
    /// <param name="companyMembershipService">Service used to resolve user company memberships.</param>
    public CurrentUserService(IHttpContextAccessor httpContextAccessor, ICompanyMembershipService companyMembershipService)
    {
        _httpContextAccessor = httpContextAccessor;
        _companyMembershipService = companyMembershipService;
    }

    /// <summary>
    /// Gets the external user identifier from JWT claims.
    /// </summary>
    public string? UserId =>
        GetClaimValue("oid") ??
        GetClaimValue(ClaimTypes.NameIdentifier) ??
        GetClaimValue("sub");

    /// <summary>
    /// Gets the user's email from JWT claims.
    /// </summary>
    public string? Email =>
        GetClaimValue("preferred_username") ??
        GetClaimValue("email") ??
        GetClaimValue(ClaimTypes.Email);

    /// <summary>
    /// Gets the company IDs for the current user.
    /// </summary>
    public IReadOnlyList<Guid> CompanyIds
    {
        get
        {
            if (!TryGetLocalUserId(out var localUserId) && !Guid.TryParse(UserId, out localUserId))
            {
                return EmptyCompanyIds;
            }

            return _companyMembershipService
                .GetCompanyIdsForUserAsync(localUserId)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }
    }

    private string? GetClaimValue(string claimType)
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst(claimType)?.Value;
    }

    private bool TryGetLocalUserId(out Guid localUserId)
    {
        localUserId = Guid.Empty;

        var items = _httpContextAccessor.HttpContext?.Items;
        if (items is null)
        {
            return false;
        }

        if (!items.TryGetValue(UserProvisioningMiddleware.LocalUserIdItemKey, out var value))
        {
            return false;
        }

        if (value is Guid id)
        {
            localUserId = id;
            return true;
        }

        return false;
    }
}
