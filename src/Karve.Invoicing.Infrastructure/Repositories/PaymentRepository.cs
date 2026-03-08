using Karve.Invoicing.Application.Interfaces;
using Karve.Invoicing.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Karve.Invoicing.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly InvoicingDbContext _context;

    public PaymentRepository(InvoicingDbContext context)
    {
        _context = context;
    }

    public async Task<Payment?> GetByIdAsync(Guid id)
    {
        return await _context.Payments.FindAsync(id);
    }

    public async Task AddAsync(Payment entity)
    {
        await _context.Payments.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Payment entity)
    {
        _context.Payments.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Payment entity)
    {
        _context.Payments.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Payment>> GetAllAsync()
    {
        return await _context.Payments.ToListAsync();
    }

    public async Task<IEnumerable<Payment>> GetByCompanyIdAsync(Guid companyId)
    {
        return await _context.Payments.Where(p => p.CompanyId == companyId).ToListAsync();
    }

    public async Task<IEnumerable<Payment>> GetByInvoiceIdAsync(Guid invoiceId)
    {
        return await _context.Payments.Where(p => p.InvoiceId == invoiceId).ToListAsync();
    }
}