using Karve.Invoicing.Domain.Enums;

namespace Karve.Invoicing.Application.DTOs;

/// <summary>
/// Data transfer object for Invoice.
/// </summary>
public class InvoiceDto
{
    /// <summary>
    /// The unique identifier of the invoice.
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// The company ID the invoice belongs to.
    /// </summary>
    public Guid CompanyId { get; set; }
    /// <summary>
    /// The customer ID for the invoice.
    /// </summary>
    public Guid CustomerId { get; set; }
    /// <summary>
    /// The invoice date.
    /// </summary>
    public DateTime InvoiceDate { get; set; }
    /// <summary>
    /// The due date for payment.
    /// </summary>
    public DateTime DueDate { get; set; }
    /// <summary>
    /// The status of the invoice.
    /// </summary>
    public InvoiceStatus Status { get; set; }
    /// <summary>
    /// The line items for the invoice.
    /// </summary>
    public ICollection<InvoiceLineItemDto> LineItems { get; set; } = new List<InvoiceLineItemDto>();
    /// <summary>
    /// The payments applied to the invoice.
    /// </summary>
    public ICollection<PaymentDto> Payments { get; set; } = new List<PaymentDto>();
}

/// <summary>
/// Request model for creating an invoice.
/// </summary>
public class CreateInvoiceRequest
{
    /// <summary>
    /// The company ID the invoice belongs to.
    /// </summary>
    public Guid CompanyId { get; set; }
    /// <summary>
    /// The customer ID for the invoice.
    /// </summary>
    public Guid CustomerId { get; set; }
    /// <summary>
    /// The invoice date.
    /// </summary>
    public DateTime InvoiceDate { get; set; }
    /// <summary>
    /// The due date for payment.
    /// </summary>
    public DateTime DueDate { get; set; }
    /// <summary>
    /// The status of the invoice.
    /// </summary>
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
}
/// <summary>
/// Request model for updating an invoice.
/// </summary>
public class UpdateInvoiceRequest
{
    /// <summary>
    /// The company ID the invoice belongs to.
    /// </summary>
    public Guid CompanyId { get; set; }
    /// <summary>
    /// The customer ID for the invoice.
    /// </summary>
    public Guid CustomerId { get; set; }
    /// <summary>
    /// The invoice date.
    /// </summary>
    public DateTime InvoiceDate { get; set; }
    /// <summary>
    /// The due date for payment.
    /// </summary>
    public DateTime DueDate { get; set; }
    /// <summary>
    /// The status of the invoice.
    /// </summary>
    public InvoiceStatus Status { get; set; }
}