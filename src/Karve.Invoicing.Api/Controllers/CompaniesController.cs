using AutoMapper;
using FluentValidation;
using Karve.Invoicing.Application.DTOs;
using Karve.Invoicing.Application.Interfaces;
using Karve.Invoicing.Application.Responses;
using Karve.Invoicing.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Karve.Invoicing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompaniesController : ControllerBase
{
    private readonly ICompanyRepository _repository;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateCompanyRequest> _createValidator;
    private readonly IValidator<UpdateCompanyRequest> _updateValidator;

    public CompaniesController(
        ICompanyRepository repository,
        IMapper mapper,
        IValidator<CreateCompanyRequest> createValidator,
        IValidator<UpdateCompanyRequest> updateValidator)
    {
        _repository = repository;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<CompanyDto>>>> Get()
    {
        try
        {
            var companies = await _repository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<CompanyDto>>(companies);
            return Ok(ApiResponse<IEnumerable<CompanyDto>>.Success(dtos));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<IEnumerable<CompanyDto>>.Failure("An error occurred while retrieving companies."));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CompanyDto>>> Get(Guid id)
    {
        try
        {
            var company = await _repository.GetByIdAsync(id);
            if (company == null)
            {
                return NotFound(ApiResponse<CompanyDto>.Failure("Company not found."));
            }

            var dto = _mapper.Map<CompanyDto>(company);
            return Ok(ApiResponse<CompanyDto>.Success(dto));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<CompanyDto>.Failure("An error occurred while retrieving the company."));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CompanyDto>>> Post([FromBody] CreateCompanyRequest request)
    {
        try
        {
            var validationResult = await _createValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(ApiResponse<CompanyDto>.Failure(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))));
            }

            var company = _mapper.Map<Company>(request);
            await _repository.AddAsync(company);
            var dto = _mapper.Map<CompanyDto>(company);
            return CreatedAtAction(nameof(Get), new { id = company.Id }, ApiResponse<CompanyDto>.Success(dto));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<CompanyDto>.Failure("An error occurred while creating the company."));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<CompanyDto>>> Put(Guid id, [FromBody] UpdateCompanyRequest request)
    {
        try
        {
            var validationResult = await _updateValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(ApiResponse<CompanyDto>.Failure(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))));
            }

            var existingCompany = await _repository.GetByIdAsync(id);
            if (existingCompany == null)
            {
                return NotFound(ApiResponse<CompanyDto>.Failure("Company not found."));
            }

            _mapper.Map(request, existingCompany);
            await _repository.UpdateAsync(existingCompany);
            var dto = _mapper.Map<CompanyDto>(existingCompany);
            return Ok(ApiResponse<CompanyDto>.Success(dto));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<CompanyDto>.Failure("An error occurred while updating the company."));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        try
        {
            var company = await _repository.GetByIdAsync(id);
            if (company == null)
            {
                return NotFound(ApiResponse<object>.Failure("Company not found."));
            }

            await _repository.DeleteAsync(company);
            return Ok(ApiResponse<object>.Success(null));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Failure("An error occurred while deleting the company."));
        }
    }
}