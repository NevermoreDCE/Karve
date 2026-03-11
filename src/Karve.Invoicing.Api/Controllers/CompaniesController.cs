using AutoMapper;
using FluentValidation;
using Karve.Invoicing.Application.DTOs;
using Karve.Invoicing.Application.Interfaces;
using Karve.Invoicing.Application.Responses;
using Karve.Invoicing.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Karve.Invoicing.Api.Controllers;

/// <summary>
/// Controller for managing companies.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]")]
public class CompaniesController : ControllerBase
{
    private readonly ICompanyRepository _repository;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateCompanyRequest> _createValidator;
    private readonly IValidator<UpdateCompanyRequest> _updateValidator;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompaniesController"/> class.
    /// </summary>
    /// <param name="repository">The company repository.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="createValidator">Validator for creating companies.</param>
    /// <param name="updateValidator">Validator for updating companies.</param>
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

    /// <summary>
    /// Gets a paginated list of companies.
    /// </summary>
    /// <param name="page">The page number (default: 1).</param>
    /// <param name="pageSize">The number of items per page (default: 20).</param>
    /// <param name="filter">Optional filter for company name.</param>
    /// <returns>A paginated list of companies.</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<CompanyDto>>>> Get(int page = 1, int pageSize = 20, string? filter = null)
    {
        try
        {
            var result = await _repository.GetPagedAsync(page, pageSize, filter);
            var dtos = _mapper.Map<IEnumerable<CompanyDto>>(result.Items);
            var pagedDto = new PagedResult<CompanyDto>
            {
                Items = dtos,
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize
            };
            return Ok(ApiResponse<PagedResult<CompanyDto>>.Success(pagedDto));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<CompanyDto>>.Failure("An error occurred while retrieving companies."));
        }
    }

    /// <summary>
    /// Gets a company by its ID.
    /// </summary>
    /// <param name="id">The company ID.</param>
    /// <returns>The company details.</returns>
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

    /// <summary>
    /// Creates a new company.
    /// </summary>
    /// <param name="request">The company creation request.</param>
    /// <returns>The created company.</returns>
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

    /// <summary>
    /// Updates an existing company.
    /// </summary>
    /// <param name="id">The company ID.</param>
    /// <param name="request">The company update request.</param>
    /// <returns>The updated company.</returns>
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

    /// <summary>
    /// Deletes a company by its ID.
    /// </summary>
    /// <param name="id">The company ID.</param>
    /// <returns>Success or failure response.</returns>
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