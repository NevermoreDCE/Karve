using FluentValidation;
using Karve.Invoicing.Application.DTOs;
using Karve.Invoicing.Domain.Enums;

namespace Karve.Invoicing.Application.Validators;

public class UpdatePaymentRequestValidator : AbstractValidator<UpdatePaymentRequest>
{
    public UpdatePaymentRequestValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty().WithMessage("Company ID is required.");

        RuleFor(x => x.InvoiceId)
            .NotEmpty().WithMessage("Invoice ID is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Payment amount must be greater than 0.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Length(3).WithMessage("Currency must be exactly 3 characters.")
            .Matches("^[A-Z]{3}$").WithMessage("Currency must be in uppercase letters (e.g., USD).");

        RuleFor(x => x.PaymentDate)
            .NotEmpty().WithMessage("Payment date is required.")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Payment date cannot be in the future.");

        RuleFor(x => x.Method)
            .IsInEnum().WithMessage("Invalid payment method.");
    }
}