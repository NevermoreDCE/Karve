using Karve.Invoicing.Domain.ValueObjects;
using Karve.Invoicing.Domain.Entities;

namespace Karve.Invoicing.Domain.Tests;

public class DomainTests
{
    [Fact]
    public void EmailAddress_ValidEmail_CreatesSuccessfully()
    {
        // Arrange
        var email = "test@example.com";

        // Act
        var emailAddress = new EmailAddress(email);

        // Assert
        Assert.Equal(email, emailAddress.Value);
    }

    [Fact]
    public void EmailAddress_InvalidEmail_ThrowsArgumentException()
    {
        // Arrange
        var invalidEmail = "invalid-email";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new EmailAddress(invalidEmail));
    }

    [Fact]
    public void EmailAddress_EmptyEmail_ThrowsArgumentException()
    {
        // Arrange
        var emptyEmail = "";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new EmailAddress(emptyEmail));
    }

    [Fact]
    public void Money_ValidAmount_CreatesSuccessfully()
    {
        // Arrange
        var amount = 100.50m;

        // Act
        var money = new Money(amount);

        // Assert
        Assert.Equal(amount, money.Amount);
        Assert.Equal("USD", money.Currency);
    }

    [Fact]
    public void Money_NegativeAmount_ThrowsArgumentException()
    {
        // Arrange
        var negativeAmount = -10.0m;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Money(negativeAmount));
    }

    [Fact]
    public void InvoiceLineItem_ValidQuantity_SetsSuccessfully()
    {
        // Arrange
        var lineItem = new InvoiceLineItem();

        // Act
        lineItem.Quantity = 5;

        // Assert
        Assert.Equal(5, lineItem.Quantity);
    }

    [Fact]
    public void InvoiceLineItem_NegativeQuantity_ThrowsArgumentException()
    {
        // Arrange
        var lineItem = new InvoiceLineItem();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => lineItem.Quantity = -1);
    }
}
