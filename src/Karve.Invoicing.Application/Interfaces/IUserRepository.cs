using Karve.Invoicing.Domain.Entities;
using Karve.Invoicing.Application.Responses;

namespace Karve.Invoicing.Application.Interfaces;

public interface IUserRepository
{
    Task<AppUser?> GetByIdAsync(Guid id);
    Task AddAsync(AppUser entity);
    Task UpdateAsync(AppUser entity);
    Task DeleteAsync(AppUser entity);
    Task<IEnumerable<AppUser>> GetAllAsync();
    Task<AppUser?> GetByExternalUserIdAsync(string externalUserId);
    Task<IEnumerable<AppUser>> GetByCompanyIdAsync(Guid companyId);
    Task<PagedResult<AppUser>> GetPagedAsync(Guid companyId, int page = 1, int pageSize = 20, string? filter = null);
}