using Karve.Invoicing.Domain.Entities;
using Karve.Invoicing.Application.Responses;

namespace Karve.Invoicing.Application.Interfaces;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id);
    Task AddAsync(Customer entity);
    Task UpdateAsync(Customer entity);
    Task DeleteAsync(Customer entity);
    Task<IEnumerable<Customer>> GetAllAsync();
    Task<IEnumerable<Customer>> GetByCompanyIdAsync(Guid companyId);
    Task<PagedResult<Customer>> GetPagedAsync(Guid companyId, int page = 1, int pageSize = 20, string? filter = null);
}