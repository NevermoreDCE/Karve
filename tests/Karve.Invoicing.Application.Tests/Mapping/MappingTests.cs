using AutoMapper;
using Karve.Invoicing.Application.DTOs;
using Karve.Invoicing.Application.Mapping;
using Karve.Invoicing.Domain.Entities;
using Karve.Invoicing.Domain.Enums;
using Karve.Invoicing.Domain.ValueObjects;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Karve.Invoicing.Application.Tests.Mapping;

public class MappingTests
{
    private readonly IMapper _mapper;

    public MappingTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<DomainToDtoProfile>();
            cfg.AddProfile<DtoToDomainProfile>();
        }, NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();
    }

    [Fact]
    public void Company_RoundTrip_Mapping()
    {
        var company = new Company { Id = Guid.NewGuid(), Name = "Acme Corp" };
        var dto = _mapper.Map<CompanyDto>(company);
        var entity = _mapper.Map<Company>(dto);
        Assert.Equal(company.Name, entity.Name);
    }

    [Fact]
    public void Customer_RoundTrip_Mapping()
    {
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            CompanyId = Guid.NewGuid(),
            Name = "John Doe",
            Email = new EmailAddress("john@example.com"),
            BillingAddress = "123 Main St"
        };
        var dto = _mapper.Map<CustomerDto>(customer);
        var entity = _mapper.Map<Customer>(dto);
        Assert.Equal(customer.Name, entity.Name);
        Assert.Equal(customer.Email.Value, entity.Email.Value);
        Assert.Equal(customer.BillingAddress, entity.BillingAddress);
    }

    [Fact]
    public void Product_RoundTrip_Mapping()
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            CompanyId = Guid.NewGuid(),
            Name = "Widget",
            Sku = "WDG-001",
            UnitPrice = new Money(19.99m, "USD")
        };
        var dto = _mapper.Map<ProductDto>(product);
        var entity = _mapper.Map<Product>(dto);
        Assert.Equal(product.Name, entity.Name);
        Assert.Equal(product.Sku, entity.Sku);
        Assert.Equal(product.UnitPrice.Amount, entity.UnitPrice.Amount);
        Assert.Equal(product.UnitPrice.Currency, entity.UnitPrice.Currency);
    }

    [Fact]
    public void Invoice_RoundTrip_Mapping()
    {
        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            CompanyId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            Status = InvoiceStatus.Draft
        };
        var dto = _mapper.Map<InvoiceDto>(invoice);
        var entity = _mapper.Map<Invoice>(dto);
        Assert.Equal(invoice.CompanyId, entity.CompanyId);
        Assert.Equal(invoice.CustomerId, entity.CustomerId);
        Assert.Equal(invoice.InvoiceDate, entity.InvoiceDate);
        Assert.Equal(invoice.DueDate, entity.DueDate);
        Assert.Equal(invoice.Status, entity.Status);
    }

    [Fact]
    public void Payment_RoundTrip_Mapping()
    {
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            CompanyId = Guid.NewGuid(),
            InvoiceId = Guid.NewGuid(),
            Amount = new Money(100.00m, "USD"),
            PaymentDate = DateTime.UtcNow,
            Method = PaymentMethod.CreditCard
        };
        var dto = _mapper.Map<PaymentDto>(payment);
        var entity = _mapper.Map<Payment>(dto);
        Assert.Equal(payment.Amount.Amount, entity.Amount.Amount);
        Assert.Equal(payment.Amount.Currency, entity.Amount.Currency);
        Assert.Equal(payment.PaymentDate, entity.PaymentDate);
        Assert.Equal(payment.Method, entity.Method);
    }
}
