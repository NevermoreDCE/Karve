using Karve.Invoicing.Application.Services;
using Microsoft.EntityFrameworkCore;

namespace Karve.Invoicing.Infrastructure.Services;

/// <summary>
/// Reads user-company memberships from persistence.
/// </summary>
public sealed class CompanyMembershipService : ICompanyMembershipService
{
    private readonly InvoicingDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompanyMembershipService"/> class.
    /// </summary>
    /// <param name="dbContext">Database context.</param>
    public CompanyMembershipService(InvoicingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Guid>> GetCompanyIdsForUserAsync(Guid userId)
    {
        return await _dbContext.CompanyUsers
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(cu => cu.UserId == userId)
            .Select(cu => cu.CompanyId)
            .Distinct()
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<bool> UserBelongsToCompanyAsync(Guid userId, Guid companyId)
    {
        return await _dbContext.CompanyUsers
            .AsNoTracking()
            .IgnoreQueryFilters()
            .AnyAsync(cu => cu.UserId == userId && cu.CompanyId == companyId);
    }
}
