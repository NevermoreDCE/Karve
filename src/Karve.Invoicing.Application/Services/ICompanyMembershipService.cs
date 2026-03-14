namespace Karve.Invoicing.Application.Services;

/// <summary>
/// Provides company-membership lookups for a local application user.
/// </summary>
public interface ICompanyMembershipService
{
    /// <summary>
    /// Gets all company IDs the user belongs to.
    /// </summary>
    /// <param name="userId">The local user ID.</param>
    /// <returns>A read-only list of company IDs.</returns>
    Task<IReadOnlyList<Guid>> GetCompanyIdsForUserAsync(Guid userId);

    /// <summary>
    /// Determines whether the user belongs to a specific company.
    /// </summary>
    /// <param name="userId">The local user ID.</param>
    /// <param name="companyId">The company ID to evaluate.</param>
    /// <returns><c>true</c> if membership exists; otherwise <c>false</c>.</returns>
    Task<bool> UserBelongsToCompanyAsync(Guid userId, Guid companyId);
}
