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
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="CurrentUserService"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">Accessor for the active HTTP context.</param>
    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
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
            var items = _httpContextAccessor.HttpContext?.Items;
            if (items is null)
            {
                return EmptyCompanyIds;
            }

            if (!items.TryGetValue(TenantResolutionMiddleware.ResolvedCompanyIdsItemKey, out var value))
            {
                return EmptyCompanyIds;
            }

            if (value is IReadOnlyList<Guid> companyIds)
            {
                return companyIds;
            }

            if (value is List<Guid> mutableCompanyIds)
            {
                return mutableCompanyIds;
            }

            return EmptyCompanyIds;
        }
    }

    private string? GetClaimValue(string claimType)
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst(claimType)?.Value;
    }
}
