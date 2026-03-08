using Karve.Invoicing.Application.Interfaces;
using Karve.Invoicing.Application.Responses;
using Karve.Invoicing.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Karve.Invoicing.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly InvoicingDbContext _context;

    public CustomerRepository(InvoicingDbContext context)
    {
        _context = context;
    }

    public async Task<Customer?> GetByIdAsync(Guid id)
    {
        return await _context.Customers.FindAsync(id);
    }

    public async Task AddAsync(Customer entity)
    {
        await _context.Customers.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Customer entity)
    {
        _context.Customers.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Customer entity)
    {
        _context.Customers.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Customer>> GetAllAsync()
    {
        return await _context.Customers.ToListAsync();
    }

    public async Task<IEnumerable<Customer>> GetByCompanyIdAsync(Guid companyId)
    {
        return await _context.Customers.Where(c => c.CompanyId == companyId).ToListAsync();
    }

    public async Task<PagedResult<Customer>> GetPagedAsync(Guid companyId, int page = 1, int pageSize = 20, string? filter = null)
    {
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
}