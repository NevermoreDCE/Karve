using Karve.Invoicing.Domain.Entities;

namespace Karve.Invoicing.Application.Interfaces;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id);
    Task AddAsync(Customer entity);
    Task UpdateAsync(Customer entity);
    Task DeleteAsync(Customer entity);
    Task<IEnumerable<Customer>> GetAllAsync();
    Task<IEnumerable<Customer>> GetByCompanyIdAsync(Guid companyId);
}