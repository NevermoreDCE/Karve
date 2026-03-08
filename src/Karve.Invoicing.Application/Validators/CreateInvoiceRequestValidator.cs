using FluentValidation;
using Karve.Invoicing.Application.DTOs;
using Karve.Invoicing.Domain.Enums;

namespace Karve.Invoicing.Application.Validators;

public class CreateInvoiceRequestValidator : AbstractValidator<CreateInvoiceRequest>
{
    public CreateInvoiceRequestValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty().WithMessage("Company ID is required.");

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");

        RuleFor(x => x.InvoiceDate)
            .NotEmpty().WithMessage("Invoice date is required.")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Invoice date cannot be in the future.");

        RuleFor(x => x.DueDate)
            .NotEmpty().WithMessage("Due date is required.")
            .GreaterThan(x => x.InvoiceDate).WithMessage("Due date must be after the invoice date.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid invoice status.");
    }
}