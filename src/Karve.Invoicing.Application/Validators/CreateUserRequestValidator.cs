using FluentValidation;
using Karve.Invoicing.Application.DTOs;

namespace Karve.Invoicing.Application.Validators;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.ExternalUserId)
            .NotEmpty().WithMessage("External user ID is required.")
            .Length(1, 100).WithMessage("External user ID must be between 1 and 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required.")
            .Must(role => role == "Admin" || role == "User" || role == "Viewer")
            .WithMessage("Role must be one of: Admin, User, Viewer.");
    }
}