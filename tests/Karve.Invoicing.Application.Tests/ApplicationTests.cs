using FluentValidation.TestHelper;
using Karve.Invoicing.Application.DTOs;
using Karve.Invoicing.Application.Validators;

namespace Karve.Invoicing.Application.Tests;

public class ValidatorTests
{
    private readonly CreateCompanyRequestValidator _validator = new();

    [Fact]
    public void CreateCompanyRequestValidator_ValidRequest_ShouldPass()
    {
        // Arrange
        var request = new CreateCompanyRequest { Name = "Test Company" };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateCompanyRequestValidator_EmptyName_ShouldFail()
    {
        // Arrange
        var request = new CreateCompanyRequest { Name = "" };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Company name is required.");
    }

    [Fact]
    public void CreateCompanyRequestValidator_NameTooLong_ShouldFail()
    {
        // Arrange
        var request = new CreateCompanyRequest { Name = new string('A', 101) };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Company name must be between 1 and 100 characters.");
    }
}
