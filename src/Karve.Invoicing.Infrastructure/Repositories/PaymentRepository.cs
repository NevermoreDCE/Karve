using Karve.Invoicing.Application.Interfaces;
using Karve.Invoicing.Application.Exceptions;
using Karve.Invoicing.Application.Responses;
using Karve.Invoicing.Application.Services;
using Karve.Invoicing.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Karve.Invoicing.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly InvoicingDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public PaymentRepository(InvoicingDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Payment?> GetByIdAsync(Guid id)
    {
        var payment = await _context.Payments.FindAsync(id);
        if (payment is not null)
        {
            return payment;
        }

        var unscopedPayment = await _context.Payments
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (unscopedPayment is not null && !_currentUser.CompanyIds.Contains(unscopedPayment.CompanyId))
        {
            throw new ForbiddenException("User does not belong to this company.");
        }

        return null;
    }

    public async Task AddAsync(Payment entity)
    {
        EnsureCompanyAccess(entity.CompanyId);
        await _context.Payments.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Payment entity)
    {
        EnsureCompanyAccess(entity.CompanyId);
        _context.Payments.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Payment entity)
    {
        EnsureCompanyAccess(entity.CompanyId);
        _context.Payments.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Payment>> GetAllAsync()
    {
        return await _context.Payments.ToListAsync();
    }

    public async Task<IEnumerable<Payment>> GetByCompanyIdAsync(Guid companyId)
    {
        EnsureCompanyAccess(companyId);
        return await _context.Payments.Where(p => p.CompanyId == companyId).ToListAsync();
    }

    public async Task<IEnumerable<Payment>> GetByInvoiceIdAsync(Guid invoiceId)
    {
        var invoice = await _context.Invoices
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == invoiceId);

        if (invoice is not null)
        {
            EnsureCompanyAccess(invoice.CompanyId);
        }

        return await _context.Payments.Where(p => p.InvoiceId == invoiceId).ToListAsync();
    }

    public async Task<PagedResult<Payment>> GetPagedAsync(Guid companyId, int page = 1, int pageSize = 20, string? filter = null)
    {
        EnsureCompanyAccess(companyId);
        var query = _context.Payments.Where(p => p.CompanyId == companyId);
        if (!string.IsNullOrEmpty(filter))
        {
            query = query.Where(p => p.Method.ToString().Contains(filter));
        }
        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(p => p.PaymentDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return new PagedResult<Payment>
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