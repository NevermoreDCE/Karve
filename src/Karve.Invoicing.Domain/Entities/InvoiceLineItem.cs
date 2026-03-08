using Karve.Invoicing.Domain.ValueObjects;

namespace Karve.Invoicing.Domain.Entities;

public class InvoiceLineItem
{
    public Guid Id { get; set; }

    public Guid InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    private int _quantity;
    public int Quantity
    {
        get => _quantity;
        set
        {
            if (value < 0)
                throw new ArgumentException("Quantity cannot be negative", nameof(value));
            _quantity = value;
        }
    }

    public Money UnitPrice { get; set; } = null!;
}