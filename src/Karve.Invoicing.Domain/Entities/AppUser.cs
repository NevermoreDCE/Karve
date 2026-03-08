using Karve.Invoicing.Domain.ValueObjects;

namespace Karve.Invoicing.Domain.Entities;

public class AppUser
{
    public Guid Id { get; set; }
    public string ExternalUserId { get; set; } = string.Empty;
    public EmailAddress Email { get; set; } = null!;

    // Navigation properties
    public ICollection<CompanyUser> CompanyUsers { get; set; } = new List<CompanyUser>();
}