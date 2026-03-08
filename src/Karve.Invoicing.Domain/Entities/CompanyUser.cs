using Karve.Invoicing.Domain.Enums;

namespace Karve.Invoicing.Domain.Entities;

public class CompanyUser
{
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;

    public UserRole Role { get; set; }
}