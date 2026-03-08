using Karve.Invoicing.Application.Interfaces;
using Karve.Invoicing.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Karve.Invoicing.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly InvoicingDbContext _context;

    public ProductRepository(InvoicingDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        return await _context.Products.FindAsync(id);
    }

    public async Task AddAsync(Product entity)
    {
        await _context.Products.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Product entity)
    {
        _context.Products.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Product entity)
    {
        _context.Products.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _context.Products.ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetByCompanyIdAsync(Guid companyId)
    {
        return await _context.Products.Where(p => p.CompanyId == companyId).ToListAsync();
    }
}