using Karve.Invoicing.Domain.Entities;
using Karve.Invoicing.Application.Responses;

namespace Karve.Invoicing.Application.Interfaces;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid id);
    Task AddAsync(Payment entity);
    Task UpdateAsync(Payment entity);
    Task DeleteAsync(Payment entity);
    Task<IEnumerable<Payment>> GetAllAsync();
    Task<IEnumerable<Payment>> GetByCompanyIdAsync(Guid companyId);
    Task<IEnumerable<Payment>> GetByInvoiceIdAsync(Guid invoiceId);
    Task<PagedResult<Payment>> GetPagedAsync(Guid companyId, int page = 1, int pageSize = 20, string? filter = null);
}