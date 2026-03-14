namespace Karve.Invoicing.Application.Services;

/// <summary>
/// Provides information about the currently authenticated user.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the external identity provider user identifier (for example, Azure AD <c>oid</c>).
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Gets the user's email address from authentication claims.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Gets the company IDs the current user belongs to.
    /// </summary>
    IReadOnlyList<Guid> CompanyIds { get; }
}
