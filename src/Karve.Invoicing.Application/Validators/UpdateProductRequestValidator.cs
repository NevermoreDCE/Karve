using FluentValidation;
using Karve.Invoicing.Application.DTOs;

namespace Karve.Invoicing.Application.Validators;

public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .Length(1, 100).WithMessage("Product name must be between 1 and 100 characters.");

        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("SKU is required.")
            .Length(1, 50).WithMessage("SKU must be between 1 and 50 characters.");

        RuleFor(x => x.UnitPriceAmount)
            .GreaterThan(0).WithMessage("Unit price must be greater than 0.");

        RuleFor(x => x.UnitPriceCurrency)
            .NotEmpty().WithMessage("Currency is required.")
            .Length(3).WithMessage("Currency must be exactly 3 characters.")
            .Matches("^[A-Z]{3}$").WithMessage("Currency must be in uppercase letters (e.g., USD).");
    }
}