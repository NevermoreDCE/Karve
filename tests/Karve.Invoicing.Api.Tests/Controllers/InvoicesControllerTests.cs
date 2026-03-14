using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Karve.Invoicing.Api.Controllers;
using Karve.Invoicing.Application.DTOs;
using Karve.Invoicing.Application.Interfaces;
using Karve.Invoicing.Application.Responses;
using Karve.Invoicing.Application.Services;
using Karve.Invoicing.Domain.Entities;
using Karve.Invoicing.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Karve.Invoicing.Api.Tests.Controllers;

public class InvoicesControllerTests
{
    [Fact]
    public async Task Get_ReturnsPagedInvoices()
    {
        var repository = new Mock<IInvoiceRepository>();
        var mapper = new Mock<IMapper>();
        var currentUser = MockCurrentUserWithSingleCompany(out var companyId);
        var createValidator = new Mock<IValidator<CreateInvoiceRequest>>();
        var updateValidator = new Mock<IValidator<UpdateInvoiceRequest>>();

        var invoices = new List<Invoice>
        {
            new()
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                CustomerId = Guid.NewGuid(),
                InvoiceDate = DateTime.UtcNow.AddDays(-1),
                DueDate = DateTime.UtcNow.AddDays(30),
                Status = InvoiceStatus.Sent
            },
            new()
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                CustomerId = Guid.NewGuid(),
                InvoiceDate = DateTime.UtcNow.AddDays(-2),
                DueDate = DateTime.UtcNow.AddDays(29),
                Status = InvoiceStatus.Draft
            }
        };

        repository
            .Setup(r => r.GetPagedAsync(companyId, 1, 20, null))
            .ReturnsAsync(new PagedResult<Invoice>
            {
                Items = invoices,
                TotalCount = invoices.Count,
                Page = 1,
                PageSize = 20
            });

        mapper
            .Setup(m => m.Map<IEnumerable<InvoiceDto>>(It.IsAny<IEnumerable<Invoice>>()))
            .Returns((IEnumerable<Invoice> source) => source.Select(i => new InvoiceDto
            {
                Id = i.Id,
                CompanyId = i.CompanyId,
                CustomerId = i.CustomerId,
                InvoiceDate = i.InvoiceDate,
                DueDate = i.DueDate,
                Status = i.Status
            }).ToList());

        var controller = new InvoicesController(repository.Object, mapper.Object, currentUser.Object, createValidator.Object, updateValidator.Object);

        var result = await controller.Get(1, 20, null);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<PagedResult<InvoiceDto>>>(ok.Value);
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.Equal(2, response.Data.Items.Count());
    }

    [Fact]
    public async Task Post_ValidRequest_ReturnsCreatedInvoice()
    {
        var repository = new Mock<IInvoiceRepository>();
        var mapper = new Mock<IMapper>();
        var currentUser = MockCurrentUserWithSingleCompany(out var companyId);
        var createValidator = new Mock<IValidator<CreateInvoiceRequest>>();
        var updateValidator = new Mock<IValidator<UpdateInvoiceRequest>>();

        var request = new CreateInvoiceRequest
        {
            CustomerId = Guid.NewGuid(),
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            Status = InvoiceStatus.Draft
        };

        var entity = new Invoice
        {
            Id = Guid.NewGuid(),
            CompanyId = Guid.Empty,
            CustomerId = request.CustomerId,
            InvoiceDate = request.InvoiceDate,
            DueDate = request.DueDate,
            Status = request.Status
        };

        createValidator
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        mapper
            .Setup(m => m.Map<Invoice>(request))
            .Returns(entity);

        mapper
            .Setup(m => m.Map<InvoiceDto>(entity))
            .Returns(() => new InvoiceDto
            {
                Id = entity.Id,
                CompanyId = entity.CompanyId,
                CustomerId = entity.CustomerId,
                InvoiceDate = entity.InvoiceDate,
                DueDate = entity.DueDate,
                Status = entity.Status
            });

        var controller = new InvoicesController(repository.Object, mapper.Object, currentUser.Object, createValidator.Object, updateValidator.Object);

        var result = await controller.Post(request);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<ApiResponse<InvoiceDto>>(created.Value);
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.Equal(companyId, response.Data.CompanyId);
        Assert.Equal(request.CustomerId, response.Data.CustomerId);

        repository.Verify(r => r.AddAsync(entity), Times.Once);
    }

    [Fact]
    public async Task Post_InvalidRequest_ReturnsBadRequest()
    {
        var repository = new Mock<IInvoiceRepository>();
        var mapper = new Mock<IMapper>();
        var currentUser = MockCurrentUserWithSingleCompany(out _);
        var createValidator = new Mock<IValidator<CreateInvoiceRequest>>();
        var updateValidator = new Mock<IValidator<UpdateInvoiceRequest>>();

        var request = new CreateInvoiceRequest
        {
            CustomerId = Guid.NewGuid(),
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(-1),
            Status = InvoiceStatus.Draft
        };

        createValidator
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("DueDate", "Due date must be after invoice date") }));

        var controller = new InvoicesController(repository.Object, mapper.Object, currentUser.Object, createValidator.Object, updateValidator.Object);

        var result = await controller.Post(request);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<InvoiceDto>>(badRequest.Value);
        Assert.False(response.IsSuccess);
        Assert.Contains("Due date must be after invoice date", response.Error);
        repository.Verify(r => r.AddAsync(It.IsAny<Invoice>()), Times.Never);
    }

    [Fact]
    public async Task Put_ValidRequest_ReturnsUpdatedInvoice()
    {
        var repository = new Mock<IInvoiceRepository>();
        var mapper = new Mock<IMapper>();
        var currentUser = MockCurrentUserWithSingleCompany(out var companyId);
        var createValidator = new Mock<IValidator<CreateInvoiceRequest>>();
        var updateValidator = new Mock<IValidator<UpdateInvoiceRequest>>();

        var id = Guid.NewGuid();
        var request = new UpdateInvoiceRequest
        {
            CustomerId = Guid.NewGuid(),
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(15),
            Status = InvoiceStatus.Sent
        };

        var existing = new Invoice
        {
            Id = id,
            CompanyId = companyId,
            CustomerId = request.CustomerId,
            InvoiceDate = request.InvoiceDate.AddDays(-2),
            DueDate = request.DueDate.AddDays(-2),
            Status = InvoiceStatus.Draft
        };

        updateValidator
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        repository
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(existing);

        mapper
            .Setup(m => m.Map<InvoiceDto>(existing))
            .Returns(new InvoiceDto
            {
                Id = id,
                CompanyId = companyId,
                CustomerId = request.CustomerId,
                InvoiceDate = request.InvoiceDate,
                DueDate = request.DueDate,
                Status = request.Status
            });

        var controller = new InvoicesController(repository.Object, mapper.Object, currentUser.Object, createValidator.Object, updateValidator.Object);

        var result = await controller.Put(id, request);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<InvoiceDto>>(ok.Value);
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.Equal(id, response.Data.Id);
        repository.Verify(r => r.UpdateAsync(existing), Times.Once);
    }

    [Fact]
    public async Task Put_InvalidRequest_ReturnsBadRequest()
    {
        var repository = new Mock<IInvoiceRepository>();
        var mapper = new Mock<IMapper>();
        var currentUser = MockCurrentUserWithSingleCompany(out _);
        var createValidator = new Mock<IValidator<CreateInvoiceRequest>>();
        var updateValidator = new Mock<IValidator<UpdateInvoiceRequest>>();

        var id = Guid.NewGuid();
        var request = new UpdateInvoiceRequest
        {
            CustomerId = Guid.NewGuid(),
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(-1),
            Status = InvoiceStatus.Draft
        };

        updateValidator
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("DueDate", "Due date must be after invoice date") }));

        var controller = new InvoicesController(repository.Object, mapper.Object, currentUser.Object, createValidator.Object, updateValidator.Object);

        var result = await controller.Put(id, request);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<InvoiceDto>>(badRequest.Value);
        Assert.False(response.IsSuccess);
        Assert.Contains("Due date must be after invoice date", response.Error);
        repository.Verify(r => r.UpdateAsync(It.IsAny<Invoice>()), Times.Never);
    }

    [Fact]
    public async Task Delete_ExistingInvoice_ReturnsOk()
    {
        var repository = new Mock<IInvoiceRepository>();
        var mapper = new Mock<IMapper>();
        var currentUser = MockCurrentUserWithSingleCompany(out var companyId);
        var createValidator = new Mock<IValidator<CreateInvoiceRequest>>();
        var updateValidator = new Mock<IValidator<UpdateInvoiceRequest>>();

        var id = Guid.NewGuid();
        var invoice = new Invoice
        {
            Id = id,
            CompanyId = companyId,
            CustomerId = Guid.NewGuid(),
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            Status = InvoiceStatus.Draft
        };

        repository
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(invoice);

        var controller = new InvoicesController(repository.Object, mapper.Object, currentUser.Object, createValidator.Object, updateValidator.Object);

        var result = await controller.Delete(id);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<object>>(ok.Value);
        Assert.True(response.IsSuccess);
        repository.Verify(r => r.DeleteAsync(invoice), Times.Once);
    }

    private static Mock<ICurrentUserService> MockCurrentUserWithSingleCompany(out Guid companyId)
    {
        companyId = Guid.NewGuid();
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(c => c.CompanyIds).Returns(new List<Guid> { companyId });
        return currentUser;
    }
}
