using FluentValidation;
using Karve.Invoicing.Application.DTOs;

namespace Karve.Invoicing.Application.Validators;

public class UpdateInvoiceLineItemRequestValidator : AbstractValidator<UpdateInvoiceLineItemRequest>
{
    public UpdateInvoiceLineItemRequestValidator()
    {
        RuleFor(x => x.InvoiceId)
            .NotEmpty().WithMessage("Invoice ID is required.");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0.");

        RuleFor(x => x.UnitPriceAmount)
            .GreaterThan(0).WithMessage("Unit price must be greater than 0.");

        RuleFor(x => x.UnitPriceCurrency)
            .NotEmpty().WithMessage("Currency is required.")
            .Length(3).WithMessage("Currency must be exactly 3 characters.")
            .Matches("^[A-Z]{3}$").WithMessage("Currency must be in uppercase letters (e.g., USD).");
    }
}