using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Karve.Invoicing.Api.Controllers;
using Karve.Invoicing.Application.DTOs;
using Karve.Invoicing.Application.Interfaces;
using Karve.Invoicing.Application.Responses;
using Karve.Invoicing.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Karve.Invoicing.Api.Tests.Controllers;

public class CompaniesControllerTests
{
    [Fact]
    public async Task Get_ReturnsPagedCompanies()
    {
        var repository = new Mock<ICompanyRepository>();
        var mapper = new Mock<IMapper>();
        var createValidator = new Mock<IValidator<CreateCompanyRequest>>();
        var updateValidator = new Mock<IValidator<UpdateCompanyRequest>>();

        var companies = new List<Company>
        {
            new() { Id = Guid.NewGuid(), Name = "Alpha Corp" },
            new() { Id = Guid.NewGuid(), Name = "Beta LLC" }
        };

        repository
            .Setup(r => r.GetPagedAsync(1, 20, null))
            .ReturnsAsync(new PagedResult<Company>
            {
                Items = companies,
                TotalCount = companies.Count,
                Page = 1,
                PageSize = 20
            });

        mapper
            .Setup(m => m.Map<IEnumerable<CompanyDto>>(It.IsAny<IEnumerable<Company>>()))
            .Returns((IEnumerable<Company> source) => source.Select(c => new CompanyDto { Id = c.Id, Name = c.Name }).ToList());

        var controller = new CompaniesController(repository.Object, mapper.Object, createValidator.Object, updateValidator.Object);

        var result = await controller.Get(1, 20, null);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<PagedResult<CompanyDto>>>(ok.Value);
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.Equal(2, response.Data.Items.Count());
    }

    [Fact]
    public async Task Post_ValidRequest_ReturnsCreatedCompany()
    {
        var repository = new Mock<ICompanyRepository>();
        var mapper = new Mock<IMapper>();
        var createValidator = new Mock<IValidator<CreateCompanyRequest>>();
        var updateValidator = new Mock<IValidator<UpdateCompanyRequest>>();

        var request = new CreateCompanyRequest { Name = "Test Company" };
        var createdId = Guid.NewGuid();
        var entity = new Company { Id = createdId, Name = request.Name };

        createValidator
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        mapper
            .Setup(m => m.Map<Company>(request))
            .Returns(entity);

        mapper
            .Setup(m => m.Map<CompanyDto>(entity))
            .Returns(new CompanyDto { Id = createdId, Name = request.Name });

        var controller = new CompaniesController(repository.Object, mapper.Object, createValidator.Object, updateValidator.Object);

        var result = await controller.Post(request);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<ApiResponse<CompanyDto>>(created.Value);
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.Equal(request.Name, response.Data.Name);

        repository.Verify(r => r.AddAsync(entity), Times.Once);
    }

    [Fact]
    public async Task Post_InvalidRequest_ReturnsBadRequest()
    {
        var repository = new Mock<ICompanyRepository>();
        var mapper = new Mock<IMapper>();
        var createValidator = new Mock<IValidator<CreateCompanyRequest>>();
        var updateValidator = new Mock<IValidator<UpdateCompanyRequest>>();

        var request = new CreateCompanyRequest { Name = string.Empty };
        createValidator
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Name", "Name is required") }));

        var controller = new CompaniesController(repository.Object, mapper.Object, createValidator.Object, updateValidator.Object);

        var result = await controller.Post(request);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<CompanyDto>>(badRequest.Value);
        Assert.False(response.IsSuccess);
        Assert.Contains("Name is required", response.Error);
        repository.Verify(r => r.AddAsync(It.IsAny<Company>()), Times.Never);
    }

    [Fact]
    public async Task Put_ValidRequest_ReturnsUpdatedCompany()
    {
        var repository = new Mock<ICompanyRepository>();
        var mapper = new Mock<IMapper>();
        var createValidator = new Mock<IValidator<CreateCompanyRequest>>();
        var updateValidator = new Mock<IValidator<UpdateCompanyRequest>>();

        var id = Guid.NewGuid();
        var request = new UpdateCompanyRequest { Name = "Updated Co" };
        var existing = new Company { Id = id, Name = "Original" };

        updateValidator
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        repository
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(existing);

        mapper
            .Setup(m => m.Map<CompanyDto>(existing))
            .Returns(new CompanyDto { Id = id, Name = request.Name });

        var controller = new CompaniesController(repository.Object, mapper.Object, createValidator.Object, updateValidator.Object);

        var result = await controller.Put(id, request);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<CompanyDto>>(ok.Value);
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.Equal(id, response.Data.Id);
        repository.Verify(r => r.UpdateAsync(existing), Times.Once);
    }

    [Fact]
    public async Task Put_InvalidRequest_ReturnsBadRequest()
    {
        var repository = new Mock<ICompanyRepository>();
        var mapper = new Mock<IMapper>();
        var createValidator = new Mock<IValidator<CreateCompanyRequest>>();
        var updateValidator = new Mock<IValidator<UpdateCompanyRequest>>();

        var id = Guid.NewGuid();
        var request = new UpdateCompanyRequest { Name = string.Empty };
        updateValidator
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Name", "Name is required") }));

        var controller = new CompaniesController(repository.Object, mapper.Object, createValidator.Object, updateValidator.Object);

        var result = await controller.Put(id, request);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<CompanyDto>>(badRequest.Value);
        Assert.False(response.IsSuccess);
        Assert.Contains("Name is required", response.Error);
        repository.Verify(r => r.UpdateAsync(It.IsAny<Company>()), Times.Never);
    }

    [Fact]
    public async Task Delete_ExistingCompany_ReturnsOk()
    {
        var repository = new Mock<ICompanyRepository>();
        var mapper = new Mock<IMapper>();
        var createValidator = new Mock<IValidator<CreateCompanyRequest>>();
        var updateValidator = new Mock<IValidator<UpdateCompanyRequest>>();

        var id = Guid.NewGuid();
        var company = new Company { Id = id, Name = "Delete Me" };

        repository
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(company);

        var controller = new CompaniesController(repository.Object, mapper.Object, createValidator.Object, updateValidator.Object);

        var result = await controller.Delete(id);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<object>>(ok.Value);
        Assert.True(response.IsSuccess);
        repository.Verify(r => r.DeleteAsync(company), Times.Once);
    }
}
