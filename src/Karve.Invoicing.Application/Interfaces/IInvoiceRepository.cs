using Karve.Invoicing.Domain.Entities;
using Karve.Invoicing.Application.Responses;

namespace Karve.Invoicing.Application.Interfaces;

public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(Guid id);
    Task AddAsync(Invoice entity);
    Task UpdateAsync(Invoice entity);
    Task DeleteAsync(Invoice entity);
    Task<IEnumerable<Invoice>> GetAllAsync();
    Task<IEnumerable<Invoice>> GetByCompanyIdAsync(Guid companyId);
    Task<IEnumerable<Invoice>> GetByCustomerIdAsync(Guid customerId);
    Task<PagedResult<Invoice>> GetPagedAsync(Guid companyId, int page = 1, int pageSize = 20, string? filter = null);
}