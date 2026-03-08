using Karve.Invoicing.Domain.Enums;

namespace Karve.Invoicing.Application.DTOs;

public class PaymentDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime PaymentDate { get; set; }
    public PaymentMethod Method { get; set; }
}

public class CreatePaymentRequest
{
    public Guid CompanyId { get; set; }
    public Guid InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime PaymentDate { get; set; }
    public PaymentMethod Method { get; set; }
}

public class UpdatePaymentRequest
{
    public Guid CompanyId { get; set; }
    public Guid InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime PaymentDate { get; set; }
    public PaymentMethod Method { get; set; }
}