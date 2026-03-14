using Karve.Invoicing.Application.Interfaces;
using Karve.Invoicing.Application.Exceptions;
using Karve.Invoicing.Application.Responses;
using Karve.Invoicing.Application.Services;
using Karve.Invoicing.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Karve.Invoicing.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly InvoicingDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UserRepository(InvoicingDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<AppUser?> GetByIdAsync(Guid id)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == id)
            .Where(u => u.CompanyUsers.Any(cu => _currentUser.CompanyIds.Contains(cu.CompanyId)))
            .FirstOrDefaultAsync();

        if (user is not null)
        {
            return user;
        }

        var unscopedExists = await _context.Users
            .IgnoreQueryFilters()
            .AsNoTracking()
            .AnyAsync(u => u.Id == id);

        if (unscopedExists)
        {
            throw new ForbiddenException("User does not belong to this company.");
        }

        return null;
    }

    public async Task AddAsync(AppUser entity)
    {
        EnsureAssignedCompaniesAccessible(entity.CompanyUsers.Select(cu => cu.CompanyId));
        await _context.Users.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(AppUser entity)
    {
        await EnsureUserAccessibleAsync(entity.Id);
        EnsureAssignedCompaniesAccessible(entity.CompanyUsers.Select(cu => cu.CompanyId));
        _context.Users.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(AppUser entity)
    {
        await EnsureUserAccessibleAsync(entity.Id);
        _context.Users.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<AppUser>> GetAllAsync()
    {
        return await _context.Users
            .Where(u => u.CompanyUsers.Any(cu => _currentUser.CompanyIds.Contains(cu.CompanyId)))
            .ToListAsync();
    }

    public async Task<AppUser?> GetByExternalUserIdAsync(string externalUserId)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.ExternalUserId == externalUserId);
    }

    public async Task<IEnumerable<AppUser>> GetByCompanyIdAsync(Guid companyId)
    {
        EnsureCompanyAccess(companyId);
        return await _context.Users.Where(u => u.CompanyUsers.Any(cu => cu.CompanyId == companyId)).ToListAsync();
    }

    public async Task<PagedResult<AppUser>> GetPagedAsync(Guid companyId, int page = 1, int pageSize = 20, string? filter = null)
    {
        EnsureCompanyAccess(companyId);
        var query = _context.Users.Where(u => u.CompanyUsers.Any(cu => cu.CompanyId == companyId));
        if (!string.IsNullOrEmpty(filter))
        {
            query = query.Where(u => u.Email.Value.Contains(filter));
        }
        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(u => u.Email)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return new PagedResult<AppUser>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    private void EnsureCompanyAccess(Guid companyId)
    {
        if (!_currentUser.CompanyIds.Contains(companyId))
        {
            throw new ForbiddenException("User does not belong to this company.");
        }
    }

    private void EnsureAssignedCompaniesAccessible(IEnumerable<Guid> companyIds)
    {
        foreach (var companyId in companyIds.Distinct())
        {
            EnsureCompanyAccess(companyId);
        }
    }

    private async Task EnsureUserAccessibleAsync(Guid userId)
    {
        var hasSharedCompany = await _context.CompanyUsers
            .AsNoTracking()
            .AnyAsync(cu => cu.UserId == userId && _currentUser.CompanyIds.Contains(cu.CompanyId));

        if (!hasSharedCompany)
        {
            throw new ForbiddenException("User does not belong to this company.");
        }
    }
}