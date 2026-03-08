using Karve.Invoicing.Domain.ValueObjects;

namespace Karve.Invoicing.Domain.Entities;

public class Customer
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public EmailAddress Email { get; set; } = null!;
    public string BillingAddress { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}