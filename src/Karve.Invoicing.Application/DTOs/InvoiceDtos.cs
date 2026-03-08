using Karve.Invoicing.Domain.Enums;

namespace Karve.Invoicing.Application.DTOs;

public class InvoiceDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid CustomerId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public InvoiceStatus Status { get; set; }
    public ICollection<InvoiceLineItemDto> LineItems { get; set; } = new List<InvoiceLineItemDto>();
    public ICollection<PaymentDto> Payments { get; set; } = new List<PaymentDto>();
}

public class CreateInvoiceRequest
{
    public Guid CompanyId { get; set; }
    public Guid CustomerId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
}

public class UpdateInvoiceRequest
{
    public Guid CompanyId { get; set; }
    public Guid CustomerId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public InvoiceStatus Status { get; set; }
}