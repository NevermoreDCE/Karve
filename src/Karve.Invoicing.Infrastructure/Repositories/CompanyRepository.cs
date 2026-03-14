using Karve.Invoicing.Application.Interfaces;
using Karve.Invoicing.Application.Exceptions;
using Karve.Invoicing.Application.Responses;
using Karve.Invoicing.Application.Services;
using Karve.Invoicing.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Karve.Invoicing.Infrastructure.Repositories;

public class CompanyRepository : ICompanyRepository
{
    private readonly InvoicingDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CompanyRepository(InvoicingDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Company?> GetByIdAsync(Guid id)
    {
        var company = await _context.Companies.FindAsync(id);
        if (company is not null)
        {
            return company;
        }

        var unscopedCompany = await _context.Companies
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (unscopedCompany is not null && !_currentUser.CompanyIds.Contains(unscopedCompany.Id))
        {
            throw new ForbiddenException("User does not belong to this company.");
        }

        return null;
    }

    public async Task AddAsync(Company entity)
    {
        await _context.Companies.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Company entity)
    {
        EnsureCompanyAccess(entity.Id);
        _context.Companies.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Company entity)
    {
        EnsureCompanyAccess(entity.Id);
        _context.Companies.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Company>> GetAllAsync()
    {
        return await _context.Companies.ToListAsync();
    }

    public async Task<PagedResult<Company>> GetPagedAsync(int page = 1, int pageSize = 20, string? filter = null)
    {
        var query = _context.Companies.AsQueryable();
        if (!string.IsNullOrEmpty(filter))
        {
            query = query.Where(c => c.Name.Contains(filter));
        }
        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return new PagedResult<Company>
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
}