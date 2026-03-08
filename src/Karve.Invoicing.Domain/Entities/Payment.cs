using Karve.Invoicing.Domain.ValueObjects;
using Karve.Invoicing.Domain.Enums;

namespace Karve.Invoicing.Domain.Entities;

public class Payment
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public Guid InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;

    public Money Amount { get; set; } = null!;
    public DateTime PaymentDate { get; set; }
    public PaymentMethod Method { get; set; }
}