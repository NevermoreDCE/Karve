namespace Karve.Invoicing.Application.Services;

/// <summary>
/// Ensures an authenticated external user has a corresponding local application user record.
/// </summary>
public interface IUserProvisioningService
{
    /// <summary>
    /// Ensures the user exists and returns the local user ID.
    /// </summary>
    /// <param name="externalUserId">External identity provider user ID (for example Azure AD oid).</param>
    /// <param name="email">Email claim value for the user.</param>
    /// <returns>The local <see cref="Guid"/> identifier for the provisioned user.</returns>
    Task<Guid> EnsureUserProvisionedAsync(string externalUserId, string email);
}
