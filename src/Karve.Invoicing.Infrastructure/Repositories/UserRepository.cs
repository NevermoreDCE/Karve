using Karve.Invoicing.Application.Interfaces;
using Karve.Invoicing.Application.Responses;
using Karve.Invoicing.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Karve.Invoicing.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly InvoicingDbContext _context;

    public UserRepository(InvoicingDbContext context)
    {
        _context = context;
    }

    public async Task<AppUser?> GetByIdAsync(Guid id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task AddAsync(AppUser entity)
    {
        await _context.Users.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(AppUser entity)
    {
        _context.Users.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(AppUser entity)
    {
        _context.Users.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<AppUser>> GetAllAsync()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task<AppUser?> GetByExternalUserIdAsync(string externalUserId)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.ExternalUserId == externalUserId);
    }

    public async Task<IEnumerable<AppUser>> GetByCompanyIdAsync(Guid companyId)
    {
        return await _context.Users.Where(u => u.CompanyUsers.Any(cu => cu.CompanyId == companyId)).ToListAsync();
    }

    public async Task<PagedResult<AppUser>> GetPagedAsync(Guid companyId, int page = 1, int pageSize = 20, string? filter = null)
    {
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
}