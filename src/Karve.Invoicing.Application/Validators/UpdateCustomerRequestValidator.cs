using FluentValidation;
using Karve.Invoicing.Application.DTOs;

namespace Karve.Invoicing.Application.Validators;

public class UpdateCustomerRequestValidator : AbstractValidator<UpdateCustomerRequest>
{
    public UpdateCustomerRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Customer name is required.")
            .Length(1, 100).WithMessage("Customer name must be between 1 and 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.BillingAddress)
            .NotEmpty().WithMessage("Billing address is required.")
            .Length(1, 500).WithMessage("Billing address must be between 1 and 500 characters.");
    }
}