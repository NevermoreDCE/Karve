using Karve.Invoicing.Application.Interfaces;
using Karve.Invoicing.Application.Exceptions;
using Karve.Invoicing.Application.Responses;
using Karve.Invoicing.Application.Services;
using Karve.Invoicing.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Karve.Invoicing.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly InvoicingDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CustomerRepository(InvoicingDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Customer?> GetByIdAsync(Guid id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer is not null)
        {
            return customer;
        }

        var unscopedCustomer = await _context.Customers
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (unscopedCustomer is not null && !_currentUser.CompanyIds.Contains(unscopedCustomer.CompanyId))
        {
            throw new ForbiddenException("User does not belong to this company.");
        }

        return null;
    }

    public async Task AddAsync(Customer entity)
    {
        EnsureCompanyAccess(entity.CompanyId);
        await _context.Customers.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Customer entity)
    {
        EnsureCompanyAccess(entity.CompanyId);
        _context.Customers.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Customer entity)
    {
        EnsureCompanyAccess(entity.CompanyId);
        _context.Customers.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Customer>> GetAllAsync()
    {
        return await _context.Customers.ToListAsync();
    }

    public async Task<IEnumerable<Customer>> GetByCompanyIdAsync(Guid companyId)
    {
        EnsureCompanyAccess(companyId);
        return await _context.Customers.Where(c => c.CompanyId == companyId).ToListAsync();
    }

    public async Task<PagedResult<Customer>> GetPagedAsync(Guid companyId, int page = 1, int pageSize = 20, string? filter = null)
    {
        EnsureCompanyAccess(companyId);
        var query = _context.Customers.Where(c => c.CompanyId == companyId);
        if (!string.IsNullOrEmpty(filter))
        {
            query = query.Where(c => c.Name.Contains(filter) || c.Email.Value.Contains(filter));
        }
        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return new PagedResult<Customer>
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