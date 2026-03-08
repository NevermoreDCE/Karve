using Karve.Invoicing.Application.Interfaces;
using Karve.Invoicing.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Karve.Invoicing.Infrastructure.Repositories;

public class CompanyRepository : ICompanyRepository
{
    private readonly InvoicingDbContext _context;

    public CompanyRepository(InvoicingDbContext context)
    {
        _context = context;
    }

    public async Task<Company?> GetByIdAsync(Guid id)
    {
        return await _context.Companies.FindAsync(id);
    }

    public async Task AddAsync(Company entity)
    {
        await _context.Companies.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Company entity)
    {
        _context.Companies.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Company entity)
    {
        _context.Companies.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Company>> GetAllAsync()
    {
        return await _context.Companies.ToListAsync();
    }
}