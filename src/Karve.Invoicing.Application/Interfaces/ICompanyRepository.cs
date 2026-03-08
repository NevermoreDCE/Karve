using Karve.Invoicing.Domain.Entities;

namespace Karve.Invoicing.Application.Interfaces;

public interface ICompanyRepository
{
    Task<Company?> GetByIdAsync(Guid id);
    Task AddAsync(Company entity);
    Task UpdateAsync(Company entity);
    Task DeleteAsync(Company entity);
    Task<IEnumerable<Company>> GetAllAsync();
}