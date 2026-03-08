using Karve.Invoicing.Domain.Entities;

namespace Karve.Invoicing.Application.Interfaces;

public interface IUserRepository
{
    Task<AppUser?> GetByIdAsync(Guid id);
    Task AddAsync(AppUser entity);
    Task UpdateAsync(AppUser entity);
    Task DeleteAsync(AppUser entity);
    Task<AppUser?> GetByExternalUserIdAsync(string externalUserId);
    Task<IEnumerable<AppUser>> GetByCompanyIdAsync(Guid companyId);
}