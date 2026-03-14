using Karve.Invoicing.Application.Interfaces;
using Karve.Invoicing.Application.Exceptions;
using Karve.Invoicing.Application.Responses;
using Karve.Invoicing.Application.Services;
using Karve.Invoicing.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Karve.Invoicing.Infrastructure.Repositories;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly InvoicingDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public InvoiceRepository(InvoicingDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Invoice?> GetByIdAsync(Guid id)
    {
        var invoice = await _context.Invoices.FindAsync(id);
        if (invoice is not null)
        {
            return invoice;
        }

        var unscopedInvoice = await _context.Invoices
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id);

        if (unscopedInvoice is not null && !_currentUser.CompanyIds.Contains(unscopedInvoice.CompanyId))
        {
            throw new ForbiddenException("User does not belong to this company.");
        }

        return null;
    }

    public async Task AddAsync(Invoice entity)
    {
        EnsureCompanyAccess(entity.CompanyId);
        await _context.Invoices.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Invoice entity)
    {
        EnsureCompanyAccess(entity.CompanyId);
        _context.Invoices.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Invoice entity)
    {
        EnsureCompanyAccess(entity.CompanyId);
        _context.Invoices.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Invoice>> GetAllAsync()
    {
        return await _context.Invoices.ToListAsync();
    }

    public async Task<IEnumerable<Invoice>> GetByCompanyIdAsync(Guid companyId)
    {
        EnsureCompanyAccess(companyId);
        return await _context.Invoices.Where(i => i.CompanyId == companyId).ToListAsync();
    }

    public async Task<IEnumerable<Invoice>> GetByCustomerIdAsync(Guid customerId)
    {
        return await _context.Invoices.Where(i => i.CustomerId == customerId).ToListAsync();
    }

    public async Task<PagedResult<Invoice>> GetPagedAsync(Guid companyId, int page = 1, int pageSize = 20, string? filter = null)
    {
        EnsureCompanyAccess(companyId);
        var query = _context.Invoices.Where(i => i.CompanyId == companyId);
        if (!string.IsNullOrEmpty(filter))
        {
            query = query.Where(i => i.Status.ToString().Contains(filter));
        }
        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(i => i.InvoiceDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return new PagedResult<Invoice>
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