using FluentValidation;
using Karve.Invoicing.Application.DTOs;

namespace Karve.Invoicing.Application.Validators;

public class CreateCompanyRequestValidator : AbstractValidator<CreateCompanyRequest>
{
    public CreateCompanyRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Company name is required.")
            .Length(1, 100).WithMessage("Company name must be between 1 and 100 characters.");
    }
}