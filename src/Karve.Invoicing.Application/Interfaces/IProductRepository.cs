using Karve.Invoicing.Domain.Entities;

namespace Karve.Invoicing.Application.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id);
    Task AddAsync(Product entity);
    Task UpdateAsync(Product entity);
    Task DeleteAsync(Product entity);
    Task<IEnumerable<Product>> GetAllAsync();
    Task<IEnumerable<Product>> GetByCompanyIdAsync(Guid companyId);
}