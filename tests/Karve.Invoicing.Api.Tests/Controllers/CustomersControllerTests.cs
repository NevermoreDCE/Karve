using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Karve.Invoicing.Api.Controllers;
using Karve.Invoicing.Application.DTOs;
using Karve.Invoicing.Application.Interfaces;
using Karve.Invoicing.Application.Responses;
using Karve.Invoicing.Application.Services;
using Karve.Invoicing.Domain.Entities;
using Karve.Invoicing.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Karve.Invoicing.Api.Tests.Controllers;

public class CustomersControllerTests
{
    [Fact]
    public async Task Get_ReturnsPagedCustomers()
    {
        var repository = new Mock<ICustomerRepository>();
        var mapper = new Mock<IMapper>();
        var currentUser = MockCurrentUserWithSingleCompany(out var companyId);
        var createValidator = new Mock<IValidator<CreateCustomerRequest>>();
        var updateValidator = new Mock<IValidator<UpdateCustomerRequest>>();

        var customers = new List<Customer>
        {
            new() { Id = Guid.NewGuid(), CompanyId = companyId, Name = "Customer 1", Email = new EmailAddress("one@test.com"), BillingAddress = "Address 1" },
            new() { Id = Guid.NewGuid(), CompanyId = companyId, Name = "Customer 2", Email = new EmailAddress("two@test.com"), BillingAddress = "Address 2" }
        };

        repository
            .Setup(r => r.GetPagedAsync(companyId, 1, 20, null))
            .ReturnsAsync(new PagedResult<Customer>
            {
                Items = customers,
                TotalCount = customers.Count,
                Page = 1,
                PageSize = 20
            });

        mapper
            .Setup(m => m.Map<IEnumerable<CustomerDto>>(It.IsAny<IEnumerable<Customer>>()))
            .Returns((IEnumerable<Customer> source) => source.Select(c => new CustomerDto
            {
                Id = c.Id,
                CompanyId = c.CompanyId,
                Name = c.Name,
                Email = c.Email.ToString(),
                BillingAddress = c.BillingAddress
            }).ToList());

        var controller = new CustomersController(repository.Object, mapper.Object, currentUser.Object, createValidator.Object, updateValidator.Object);

        var result = await controller.Get(1, 20, null);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<PagedResult<CustomerDto>>>(ok.Value);
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.Equal(2, response.Data.Items.Count());
    }

    [Fact]
    public async Task Post_ValidRequest_ReturnsCreatedCustomer()
    {
        var repository = new Mock<ICustomerRepository>();
        var mapper = new Mock<IMapper>();
        var currentUser = MockCurrentUserWithSingleCompany(out var companyId);
        var createValidator = new Mock<IValidator<CreateCustomerRequest>>();
        var updateValidator = new Mock<IValidator<UpdateCustomerRequest>>();

        var request = new CreateCustomerRequest
        {
            Name = "John Doe",
            Email = "john@example.com",
            BillingAddress = "123 Main St"
        };

        var entity = new Customer
        {
            Id = Guid.NewGuid(),
            CompanyId = Guid.Empty,
            Name = request.Name,
            Email = new EmailAddress(request.Email),
            BillingAddress = request.BillingAddress
        };

        createValidator
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        mapper
            .Setup(m => m.Map<Customer>(request))
            .Returns(entity);

        mapper
            .Setup(m => m.Map<CustomerDto>(entity))
            .Returns(() => new CustomerDto
            {
                Id = entity.Id,
                CompanyId = entity.CompanyId,
                Name = entity.Name,
                Email = entity.Email.ToString(),
                BillingAddress = entity.BillingAddress
            });

        var controller = new CustomersController(repository.Object, mapper.Object, currentUser.Object, createValidator.Object, updateValidator.Object);

        var result = await controller.Post(request);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<ApiResponse<CustomerDto>>(created.Value);
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.Equal(request.Name, response.Data.Name);
        Assert.Equal(companyId, response.Data.CompanyId);

        repository.Verify(r => r.AddAsync(entity), Times.Once);
    }

    [Fact]
    public async Task Post_InvalidRequest_ReturnsBadRequest()
    {
        var repository = new Mock<ICustomerRepository>();
        var mapper = new Mock<IMapper>();
        var currentUser = MockCurrentUserWithSingleCompany(out _);
        var createValidator = new Mock<IValidator<CreateCustomerRequest>>();
        var updateValidator = new Mock<IValidator<UpdateCustomerRequest>>();

        var request = new CreateCustomerRequest { Name = string.Empty, Email = "bad", BillingAddress = string.Empty };
        createValidator
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Name", "Name is required") }));

        var controller = new CustomersController(repository.Object, mapper.Object, currentUser.Object, createValidator.Object, updateValidator.Object);

        var result = await controller.Post(request);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<CustomerDto>>(badRequest.Value);
        Assert.False(response.IsSuccess);
        Assert.Contains("Name is required", response.Error);
        repository.Verify(r => r.AddAsync(It.IsAny<Customer>()), Times.Never);
    }

    [Fact]
    public async Task Put_ValidRequest_ReturnsUpdatedCustomer()
    {
        var repository = new Mock<ICustomerRepository>();
        var mapper = new Mock<IMapper>();
        var currentUser = MockCurrentUserWithSingleCompany(out var companyId);
        var createValidator = new Mock<IValidator<CreateCustomerRequest>>();
        var updateValidator = new Mock<IValidator<UpdateCustomerRequest>>();

        var id = Guid.NewGuid();
        var request = new UpdateCustomerRequest
        {
            Name = "Updated Customer",
            Email = "updated@example.com",
            BillingAddress = "Updated Address"
        };

        var existing = new Customer
        {
            Id = id,
            CompanyId = companyId,
            Name = "Original",
            Email = new EmailAddress("original@example.com"),
            BillingAddress = "Original Address"
        };

        updateValidator
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        repository
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(existing);

        mapper
            .Setup(m => m.Map<CustomerDto>(existing))
            .Returns(new CustomerDto
            {
                Id = id,
                CompanyId = companyId,
                Name = request.Name,
                Email = request.Email,
                BillingAddress = request.BillingAddress
            });

        var controller = new CustomersController(repository.Object, mapper.Object, currentUser.Object, createValidator.Object, updateValidator.Object);

        var result = await controller.Put(id, request);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<CustomerDto>>(ok.Value);
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.Equal(id, response.Data.Id);
        repository.Verify(r => r.UpdateAsync(existing), Times.Once);
    }

    [Fact]
    public async Task Put_InvalidRequest_ReturnsBadRequest()
    {
        var repository = new Mock<ICustomerRepository>();
        var mapper = new Mock<IMapper>();
        var currentUser = MockCurrentUserWithSingleCompany(out _);
        var createValidator = new Mock<IValidator<CreateCustomerRequest>>();
        var updateValidator = new Mock<IValidator<UpdateCustomerRequest>>();

        var id = Guid.NewGuid();
        var request = new UpdateCustomerRequest { Name = string.Empty, Email = "bad", BillingAddress = string.Empty };
        updateValidator
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Name", "Name is required") }));

        var controller = new CustomersController(repository.Object, mapper.Object, currentUser.Object, createValidator.Object, updateValidator.Object);

        var result = await controller.Put(id, request);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<CustomerDto>>(badRequest.Value);
        Assert.False(response.IsSuccess);
        Assert.Contains("Name is required", response.Error);
        repository.Verify(r => r.UpdateAsync(It.IsAny<Customer>()), Times.Never);
    }

    [Fact]
    public async Task Delete_ExistingCustomer_ReturnsOk()
    {
        var repository = new Mock<ICustomerRepository>();
        var mapper = new Mock<IMapper>();
        var currentUser = MockCurrentUserWithSingleCompany(out var companyId);
        var createValidator = new Mock<IValidator<CreateCustomerRequest>>();
        var updateValidator = new Mock<IValidator<UpdateCustomerRequest>>();

        var id = Guid.NewGuid();
        var customer = new Customer
        {
            Id = id,
            CompanyId = companyId,
            Name = "Delete Me",
            Email = new EmailAddress("delete@example.com"),
            BillingAddress = "Delete Address"
        };

        repository
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(customer);

        var controller = new CustomersController(repository.Object, mapper.Object, currentUser.Object, createValidator.Object, updateValidator.Object);

        var result = await controller.Delete(id);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<object>>(ok.Value);
        Assert.True(response.IsSuccess);
        repository.Verify(r => r.DeleteAsync(customer), Times.Once);
    }

    private static Mock<ICurrentUserService> MockCurrentUserWithSingleCompany(out Guid companyId)
    {
        companyId = Guid.NewGuid();
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(c => c.CompanyIds).Returns(new List<Guid> { companyId });
        return currentUser;
    }
}
