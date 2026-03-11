using Karve.Invoicing.Domain.Enums;

namespace Karve.Invoicing.Application.DTOs;

/// <summary>
/// Data transfer object for Payment.
/// </summary>
public class PaymentDto
{
    /// <summary>
    /// The unique identifier of the payment.
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// The company ID the payment belongs to.
    /// </summary>
    public Guid CompanyId { get; set; }
    /// <summary>
    /// The invoice ID the payment is applied to.
    /// </summary>
    public Guid InvoiceId { get; set; }
    /// <summary>
    /// The payment amount.
    /// </summary>
    public decimal Amount { get; set; }
    /// <summary>
    /// The payment currency.
    /// </summary>
    public string Currency { get; set; } = "USD";
    /// <summary>
    /// The date of the payment.
    /// </summary>
    public DateTime PaymentDate { get; set; }
    /// <summary>
    /// The payment method.
    /// </summary>
    public PaymentMethod Method { get; set; }
}

/// <summary>
/// Request model for creating a payment.
/// </summary>
public class CreatePaymentRequest
{
    /// <summary>
    /// The company ID the payment belongs to.
    /// </summary>
    public Guid CompanyId { get; set; }
    /// <summary>
    /// The invoice ID the payment is applied to.
    /// </summary>
    public Guid InvoiceId { get; set; }
    /// <summary>
    /// The payment amount.
    /// </summary>
    public decimal Amount { get; set; }
    /// <summary>
    /// The payment currency.
    /// </summary>
    public string Currency { get; set; } = "USD";
    /// <summary>
    /// The date of the payment.
    /// </summary>
    public DateTime PaymentDate { get; set; }
    /// <summary>
    /// The payment method.
    /// </summary>
    public PaymentMethod Method { get; set; }
}

/// <summary>
/// Request model for updating a payment.
/// </summary>
public class UpdatePaymentRequest
{
    /// <summary>
    /// The company ID the payment belongs to.
    /// </summary>
    public Guid CompanyId { get; set; }
    /// <summary>
    /// The invoice ID the payment is applied to.
    /// </summary>
    public Guid InvoiceId { get; set; }
    /// <summary>
    /// The payment amount.
    /// </summary>
    public decimal Amount { get; set; }
    /// <summary>
    /// The payment currency.
    /// </summary>
    public string Currency { get; set; } = "USD";
    /// <summary>
    /// The date of the payment.
    /// </summary>
    public DateTime PaymentDate { get; set; }
    /// <summary>
    /// The payment method.
    /// </summary>
    public PaymentMethod Method { get; set; }
}