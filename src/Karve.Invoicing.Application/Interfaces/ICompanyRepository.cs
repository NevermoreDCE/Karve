using Karve.Invoicing.Domain.Entities;
using Karve.Invoicing.Application.Responses;

namespace Karve.Invoicing.Application.Interfaces;

public interface ICompanyRepository
{
    Task<Company?> GetByIdAsync(Guid id);
    Task AddAsync(Company entity);
    Task UpdateAsync(Company entity);
    Task DeleteAsync(Company entity);
    Task<IEnumerable<Company>> GetAllAsync();
    Task<PagedResult<Company>> GetPagedAsync(int page = 1, int pageSize = 20, string? filter = null);
}