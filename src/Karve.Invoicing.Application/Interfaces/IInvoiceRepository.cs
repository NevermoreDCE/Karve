using Karve.Invoicing.Domain.Entities;

namespace Karve.Invoicing.Application.Interfaces;

public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(Guid id);
    Task AddAsync(Invoice entity);
    Task UpdateAsync(Invoice entity);
    Task DeleteAsync(Invoice entity);
    Task<IEnumerable<Invoice>> GetByCompanyIdAsync(Guid companyId);
    Task<IEnumerable<Invoice>> GetByCustomerIdAsync(Guid customerId);
}