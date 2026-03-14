using FluentValidation.TestHelper;
using Karve.Invoicing.Application.DTOs;
using Karve.Invoicing.Application.Validators;
using Karve.Invoicing.Domain.Enums;
using Xunit;

namespace Karve.Invoicing.Application.Tests.Validators;

public class ValidatorTests
{
    [Fact]
    public void CreateCompanyRequest_Valid_ShouldPass()
    {
        var validator = new CreateCompanyRequestValidator();
        var request = new CreateCompanyRequest { Name = "Acme Corp" };
        var result = validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateCompanyRequest_EmptyName_ShouldFail()
    {
        var validator = new CreateCompanyRequestValidator();
        var request = new CreateCompanyRequest { Name = "" };
        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void CreateCustomerRequest_Valid_ShouldPass()
    {
        var validator = new CreateCustomerRequestValidator();
        var request = new CreateCustomerRequest
        {
            Name = "John Doe",
            Email = "john@example.com",
            BillingAddress = "123 Main St"
        };
        var result = validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateCustomerRequest_InvalidEmail_ShouldFail()
    {
        var validator = new CreateCustomerRequestValidator();
        var request = new CreateCustomerRequest
        {
            Name = "John Doe",
            Email = "not-an-email",
            BillingAddress = "123 Main St"
        };
        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void CreateProductRequest_Valid_ShouldPass()
    {
        var validator = new CreateProductRequestValidator();
        var request = new CreateProductRequest
        {
            Name = "Widget",
            Sku = "WDG-001",
            UnitPriceAmount = 19.99m,
            UnitPriceCurrency = "USD"
        };
        var result = validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateProductRequest_NegativePrice_ShouldFail()
    {
        var validator = new CreateProductRequestValidator();
        var request = new CreateProductRequest
        {
            Name = "Widget",
            Sku = "WDG-001",
            UnitPriceAmount = -10.00m,
            UnitPriceCurrency = "USD"
        };
        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.UnitPriceAmount);
    }

    [Fact]
    public void CreateInvoiceRequest_Valid_ShouldPass()
    {
        var validator = new CreateInvoiceRequestValidator();
        var invoiceDate = DateTime.UtcNow.AddSeconds(-1);
        var request = new CreateInvoiceRequest
        {
            CustomerId = Guid.NewGuid(),
            InvoiceDate = invoiceDate,
            DueDate = invoiceDate.AddDays(30),
            Status = InvoiceStatus.Draft
        };
        var result = validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateInvoiceRequest_DueDateBeforeInvoiceDate_ShouldFail()
    {
        var validator = new CreateInvoiceRequestValidator();
        var request = new CreateInvoiceRequest
        {
            CustomerId = Guid.NewGuid(),
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(-1),
            Status = InvoiceStatus.Draft
        };
        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.DueDate);
    }
}
