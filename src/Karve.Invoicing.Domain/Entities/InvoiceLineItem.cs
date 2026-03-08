using Karve.Invoicing.Domain.ValueObjects;

namespace Karve.Invoicing.Domain.Entities;

public class InvoiceLineItem
{
    public Guid Id { get; set; }

    public Guid InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int Quantity { get; set; }
    public Money UnitPrice { get; set; } = null!;
}