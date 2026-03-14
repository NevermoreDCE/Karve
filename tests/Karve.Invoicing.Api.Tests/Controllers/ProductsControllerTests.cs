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

public class ProductsControllerTests
{
    [Fact]
    public async Task Get_ReturnsPagedProducts()
    {
        var repository = new Mock<IProductRepository>();
        var mapper = new Mock<IMapper>();
        var currentUser = MockCurrentUserWithSingleCompany(out var companyId);
        var createValidator = new Mock<IValidator<CreateProductRequest>>();
        var updateValidator = new Mock<IValidator<UpdateProductRequest>>();

        var products = new List<Product>
        {
            new() { Id = Guid.NewGuid(), CompanyId = companyId, Name = "Product 1", Sku = "SKU-1", UnitPrice = new Money(10m, "USD") },
            new() { Id = Guid.NewGuid(), CompanyId = companyId, Name = "Product 2", Sku = "SKU-2", UnitPrice = new Money(20m, "USD") }
        };

        repository
            .Setup(r => r.GetPagedAsync(companyId, 1, 20, null))
            .ReturnsAsync(new PagedResult<Product>
            {
                Items = products,
                TotalCount = products.Count,
                Page = 1,
                PageSize = 20
            });

        mapper
            .Setup(m => m.Map<IEnumerable<ProductDto>>(It.IsAny<IEnumerable<Product>>()))
            .Returns((IEnumerable<Product> source) => source.Select(p => new ProductDto
            {
                Id = p.Id,
                CompanyId = p.CompanyId,
                Name = p.Name,
                Sku = p.Sku,
                UnitPriceAmount = p.UnitPrice.Amount,
                UnitPriceCurrency = p.UnitPrice.Currency
            }).ToList());

        var controller = new ProductsController(repository.Object, mapper.Object, currentUser.Object, createValidator.Object, updateValidator.Object);

        var result = await controller.Get(1, 20, null);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<PagedResult<ProductDto>>>(ok.Value);
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.Equal(2, response.Data.Items.Count());
    }

    [Fact]
    public async Task Post_ValidRequest_ReturnsCreatedProduct()
    {
        var repository = new Mock<IProductRepository>();
        var mapper = new Mock<IMapper>();
        var currentUser = MockCurrentUserWithSingleCompany(out var companyId);
        var createValidator = new Mock<IValidator<CreateProductRequest>>();
        var updateValidator = new Mock<IValidator<UpdateProductRequest>>();

        var request = new CreateProductRequest
        {
            Name = "Widget",
            Sku = "WDG-001",
            UnitPriceAmount = 19.99m,
            UnitPriceCurrency = "USD"
        };

        var entity = new Product
        {
            Id = Guid.NewGuid(),
            CompanyId = Guid.Empty,
            Name = request.Name,
            Sku = request.Sku,
            UnitPrice = new Money(request.UnitPriceAmount, request.UnitPriceCurrency)
        };

        createValidator
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        mapper
            .Setup(m => m.Map<Product>(request))
            .Returns(entity);

        mapper
            .Setup(m => m.Map<ProductDto>(entity))
            .Returns(() => new ProductDto
            {
                Id = entity.Id,
                CompanyId = entity.CompanyId,
                Name = entity.Name,
                Sku = entity.Sku,
                UnitPriceAmount = entity.UnitPrice.Amount,
                UnitPriceCurrency = entity.UnitPrice.Currency
            });

        var controller = new ProductsController(repository.Object, mapper.Object, currentUser.Object, createValidator.Object, updateValidator.Object);

        var result = await controller.Post(request);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<ApiResponse<ProductDto>>(created.Value);
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.Equal(request.Name, response.Data.Name);
        Assert.Equal(companyId, response.Data.CompanyId);

        repository.Verify(r => r.AddAsync(entity), Times.Once);
    }

    [Fact]
    public async Task Post_InvalidRequest_ReturnsBadRequest()
    {
        var repository = new Mock<IProductRepository>();
        var mapper = new Mock<IMapper>();
        var currentUser = MockCurrentUserWithSingleCompany(out _);
        var createValidator = new Mock<IValidator<CreateProductRequest>>();
        var updateValidator = new Mock<IValidator<UpdateProductRequest>>();

        var request = new CreateProductRequest
        {
            Name = string.Empty,
            Sku = string.Empty,
            UnitPriceAmount = -1m,
            UnitPriceCurrency = "USD"
        };

        createValidator
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Name", "Name is required") }));

        var controller = new ProductsController(repository.Object, mapper.Object, currentUser.Object, createValidator.Object, updateValidator.Object);

        var result = await controller.Post(request);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<ProductDto>>(badRequest.Value);
        Assert.False(response.IsSuccess);
        Assert.Contains("Name is required", response.Error);
        repository.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task Put_ValidRequest_ReturnsUpdatedProduct()
    {
        var repository = new Mock<IProductRepository>();
        var mapper = new Mock<IMapper>();
        var currentUser = MockCurrentUserWithSingleCompany(out var companyId);
        var createValidator = new Mock<IValidator<CreateProductRequest>>();
        var updateValidator = new Mock<IValidator<UpdateProductRequest>>();

        var id = Guid.NewGuid();
        var request = new UpdateProductRequest
        {
            Name = "Updated Widget",
            Sku = "UPD-001",
            UnitPriceAmount = 49.99m,
            UnitPriceCurrency = "USD"
        };

        var existing = new Product
        {
            Id = id,
            CompanyId = companyId,
            Name = "Original Widget",
            Sku = "ORG-001",
            UnitPrice = new Money(10m, "USD")
        };

        updateValidator
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        repository
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(existing);

        mapper
            .Setup(m => m.Map<ProductDto>(existing))
            .Returns(new ProductDto
            {
                Id = id,
                CompanyId = companyId,
                Name = request.Name,
                Sku = request.Sku,
                UnitPriceAmount = request.UnitPriceAmount,
                UnitPriceCurrency = request.UnitPriceCurrency
            });

        var controller = new ProductsController(repository.Object, mapper.Object, currentUser.Object, createValidator.Object, updateValidator.Object);

        var result = await controller.Put(id, request);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<ProductDto>>(ok.Value);
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.Equal(id, response.Data.Id);
        repository.Verify(r => r.UpdateAsync(existing), Times.Once);
    }

    [Fact]
    public async Task Put_InvalidRequest_ReturnsBadRequest()
    {
        var repository = new Mock<IProductRepository>();
        var mapper = new Mock<IMapper>();
        var currentUser = MockCurrentUserWithSingleCompany(out _);
        var createValidator = new Mock<IValidator<CreateProductRequest>>();
        var updateValidator = new Mock<IValidator<UpdateProductRequest>>();

        var id = Guid.NewGuid();
        var request = new UpdateProductRequest
        {
            Name = string.Empty,
            Sku = string.Empty,
            UnitPriceAmount = -1m,
            UnitPriceCurrency = "USD"
        };

        updateValidator
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Name", "Name is required") }));

        var controller = new ProductsController(repository.Object, mapper.Object, currentUser.Object, createValidator.Object, updateValidator.Object);

        var result = await controller.Put(id, request);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<ProductDto>>(badRequest.Value);
        Assert.False(response.IsSuccess);
        Assert.Contains("Name is required", response.Error);
        repository.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task Delete_ExistingProduct_ReturnsOk()
    {
        var repository = new Mock<IProductRepository>();
        var mapper = new Mock<IMapper>();
        var currentUser = MockCurrentUserWithSingleCompany(out var companyId);
        var createValidator = new Mock<IValidator<CreateProductRequest>>();
        var updateValidator = new Mock<IValidator<UpdateProductRequest>>();

        var id = Guid.NewGuid();
        var product = new Product
        {
            Id = id,
            CompanyId = companyId,
            Name = "Delete Me",
            Sku = "DEL-001",
            UnitPrice = new Money(12m, "USD")
        };

        repository
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(product);

        var controller = new ProductsController(repository.Object, mapper.Object, currentUser.Object, createValidator.Object, updateValidator.Object);

        var result = await controller.Delete(id);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<object>>(ok.Value);
        Assert.True(response.IsSuccess);
        repository.Verify(r => r.DeleteAsync(product), Times.Once);
    }

    private static Mock<ICurrentUserService> MockCurrentUserWithSingleCompany(out Guid companyId)
    {
        companyId = Guid.NewGuid();
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(c => c.CompanyIds).Returns(new List<Guid> { companyId });
        return currentUser;
    }
}
