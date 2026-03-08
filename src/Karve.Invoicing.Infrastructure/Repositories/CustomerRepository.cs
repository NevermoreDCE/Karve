using Karve.Invoicing.Application.Interfaces;
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

    public async Task<IEnumerable<Customer>> GetByCompanyIdAsync(Guid companyId)
    {
        return await _context.Customers.Where(c => c.CompanyId == companyId).ToListAsync();
    }
}