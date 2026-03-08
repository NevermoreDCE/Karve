using Karve.Invoicing.Application.Interfaces;
using Karve.Invoicing.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Karve.Invoicing.Infrastructure.Repositories;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly InvoicingDbContext _context;

    public InvoiceRepository(InvoicingDbContext context)
    {
        _context = context;
    }

    public async Task<Invoice?> GetByIdAsync(Guid id)
    {
        return await _context.Invoices.FindAsync(id);
    }

    public async Task AddAsync(Invoice entity)
    {
        await _context.Invoices.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Invoice entity)
    {
        _context.Invoices.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Invoice entity)
    {
        _context.Invoices.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Invoice>> GetAllAsync()
    {
        return await _context.Invoices.ToListAsync();
    }

    public async Task<IEnumerable<Invoice>> GetByCompanyIdAsync(Guid companyId)
    {
        return await _context.Invoices.Where(i => i.CompanyId == companyId).ToListAsync();
    }

    public async Task<IEnumerable<Invoice>> GetByCustomerIdAsync(Guid customerId)
    {
        return await _context.Invoices.Where(i => i.CustomerId == customerId).ToListAsync();
    }
}