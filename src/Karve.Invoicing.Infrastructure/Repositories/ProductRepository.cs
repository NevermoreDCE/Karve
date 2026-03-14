using Karve.Invoicing.Application.Interfaces;
using Karve.Invoicing.Application.Exceptions;
using Karve.Invoicing.Application.Responses;
using Karve.Invoicing.Application.Services;
using Karve.Invoicing.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Karve.Invoicing.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly InvoicingDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ProductRepository(InvoicingDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product is not null)
        {
            return product;
        }

        var unscopedProduct = await _context.Products
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (unscopedProduct is not null && !_currentUser.CompanyIds.Contains(unscopedProduct.CompanyId))
        {
            throw new ForbiddenException("User does not belong to this company.");
        }

        return null;
    }

    public async Task AddAsync(Product entity)
    {
        EnsureCompanyAccess(entity.CompanyId);
        await _context.Products.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Product entity)
    {
        EnsureCompanyAccess(entity.CompanyId);
        _context.Products.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Product entity)
    {
        EnsureCompanyAccess(entity.CompanyId);
        _context.Products.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _context.Products.ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetByCompanyIdAsync(Guid companyId)
    {
        EnsureCompanyAccess(companyId);
        return await _context.Products.Where(p => p.CompanyId == companyId).ToListAsync();
    }

    public async Task<PagedResult<Product>> GetPagedAsync(Guid companyId, int page = 1, int pageSize = 20, string? filter = null)
    {
        EnsureCompanyAccess(companyId);
        var query = _context.Products.Where(p => p.CompanyId == companyId);
        if (!string.IsNullOrEmpty(filter))
        {
            query = query.Where(p => p.Name.Contains(filter) || p.Sku.Contains(filter));
        }
        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return new PagedResult<Product>
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