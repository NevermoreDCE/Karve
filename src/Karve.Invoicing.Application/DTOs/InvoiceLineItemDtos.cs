namespace Karve.Invoicing.Application.DTOs;

/// <summary>
/// Data transfer object for Invoice Line Item.
/// </summary>
public class InvoiceLineItemDto
{
    /// <summary>
    /// The unique identifier of the line item.
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// The invoice ID this line item belongs to.
    /// </summary>
    public Guid InvoiceId { get; set; }
    /// <summary>
    /// The product ID for the line item.
    /// </summary>
    public Guid ProductId { get; set; }
    /// <summary>
    /// The quantity for the line item.
    /// </summary>
    public int Quantity { get; set; }
    /// <summary>
    /// The unit price amount.
    /// </summary>
    public decimal UnitPriceAmount { get; set; }
    /// <summary>
    /// The unit price currency.
    /// </summary>
    public string UnitPriceCurrency { get; set; } = "USD";
}

/// <summary>
/// Request model for creating an invoice line item.
/// </summary>
public class CreateInvoiceLineItemRequest
{
    /// <summary>
    /// The invoice ID this line item belongs to.
    /// </summary>
    public Guid InvoiceId { get; set; }
    /// <summary>
    /// The product ID for the line item.
    /// </summary>
    public Guid ProductId { get; set; }
    /// <summary>
    /// The quantity for the line item.
    /// </summary>
    public int Quantity { get; set; }
    /// <summary>
    /// The unit price amount.
    /// </summary>
    public decimal UnitPriceAmount { get; set; }
    /// <summary>
    /// The unit price currency.
    /// </summary>
    public string UnitPriceCurrency { get; set; } = "USD";
}

/// <summary>
/// Request model for updating an invoice line item.
/// </summary>
public class UpdateInvoiceLineItemRequest
{
    /// <summary>
    /// The invoice ID this line item belongs to.
    /// </summary>
    public Guid InvoiceId { get; set; }
    /// <summary>
    /// The product ID for the line item.
    /// </summary>
    public Guid ProductId { get; set; }
    /// <summary>
    /// The quantity for the line item.
    /// </summary>
    public int Quantity { get; set; }
    /// <summary>
    /// The unit price amount.
    /// </summary>
    public decimal UnitPriceAmount { get; set; }
    /// <summary>
    /// The unit price currency.
    /// </summary>
    public string UnitPriceCurrency { get; set; } = "USD";
}