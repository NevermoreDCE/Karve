namespace Karve.Invoicing.Application.DTOs;

public class InvoiceLineItemDto
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPriceAmount { get; set; }
    public string UnitPriceCurrency { get; set; } = "USD";
}

public class CreateInvoiceLineItemRequest
{
    public Guid InvoiceId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPriceAmount { get; set; }
    public string UnitPriceCurrency { get; set; } = "USD";
}

public class UpdateInvoiceLineItemRequest
{
    public Guid InvoiceId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPriceAmount { get; set; }
    public string UnitPriceCurrency { get; set; } = "USD";
}