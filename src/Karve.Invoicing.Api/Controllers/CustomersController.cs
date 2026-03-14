using AutoMapper;
using FluentValidation;
using Karve.Invoicing.Application.DTOs;
using Karve.Invoicing.Application.Interfaces;
using Karve.Invoicing.Application.Responses;
using Karve.Invoicing.Application.Services;
using Karve.Invoicing.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Karve.Invoicing.Api.Controllers;

/// <summary>
/// Controller for managing customers.
/// </summary>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerRepository _repository;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<CreateCustomerRequest> _createValidator;
    private readonly IValidator<UpdateCustomerRequest> _updateValidator;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomersController"/> class.
    /// </summary>
    /// <param name="repository">The customer repository.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="currentUser">Current authenticated user context.</param>
    /// <param name="createValidator">Validator for creating customers.</param>
    /// <param name="updateValidator">Validator for updating customers.</param>
    public CustomersController(
        ICustomerRepository repository,
        IMapper mapper,
        ICurrentUserService currentUser,
        IValidator<CreateCustomerRequest> createValidator,
        IValidator<UpdateCustomerRequest> updateValidator)
    {
        _repository = repository;
        _mapper = mapper;
        _currentUser = currentUser;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    /// <summary>
    /// Gets a paginated list of customers for a specific company.
    /// </summary>
    /// <param name="page">The page number (default: 1).</param>
    /// <param name="pageSize">The number of items per page (default: 20).</param>
    /// <param name="filter">Optional filter for customer name or email.</param>
    /// <returns>A paginated list of customers.</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<CustomerDto>>>> Get(int page = 1, int pageSize = 20, string? filter = null)
    {
        try
        {
            if (!TryGetSingleCompanyId(out var companyId, out var contextError))
            {
                return BadRequest(ApiResponse<PagedResult<CustomerDto>>.Failure(contextError));
            }

            var result = await _repository.GetPagedAsync(companyId, page, pageSize, filter);
            var dtos = _mapper.Map<IEnumerable<CustomerDto>>(result.Items);
            var pagedDto = new PagedResult<CustomerDto>
            {
                Items = dtos,
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize
            };
            return Ok(ApiResponse<PagedResult<CustomerDto>>.Success(pagedDto));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<CustomerDto>>.Failure("An error occurred while retrieving customers."));
        }
    }

    /// <summary>
    /// Gets a customer by its ID.
    /// </summary>
    /// <param name="id">The customer ID.</param>
    /// <returns>The customer details.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> Get(Guid id)
    {
        try
        {
            var customer = await _repository.GetByIdAsync(id);
            if (customer == null)
            {
                return NotFound(ApiResponse<CustomerDto>.Failure("Customer not found."));
            }

            var dto = _mapper.Map<CustomerDto>(customer);
            return Ok(ApiResponse<CustomerDto>.Success(dto));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<CustomerDto>.Failure("An error occurred while retrieving the customer."));
        }
    }

    /// <summary>
    /// Creates a new customer.
    /// </summary>
    /// <param name="request">The customer creation request.</param>
    /// <returns>The created customer.</returns>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> Post([FromBody] CreateCustomerRequest request)
    {
        try
        {
            var validationResult = await _createValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(ApiResponse<CustomerDto>.Failure(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))));
            }

            var customer = _mapper.Map<Customer>(request);
            if (!TryGetSingleCompanyId(out var companyId, out var contextError))
            {
                return BadRequest(ApiResponse<CustomerDto>.Failure(contextError));
            }

            customer.CompanyId = companyId;
            await _repository.AddAsync(customer);
            var dto = _mapper.Map<CustomerDto>(customer);
            return CreatedAtAction(nameof(Get), new { id = customer.Id }, ApiResponse<CustomerDto>.Success(dto));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<CustomerDto>.Failure("An error occurred while creating the customer."));
        }
    }

    /// <summary>
    /// Updates an existing customer.
    /// </summary>
    /// <param name="id">The customer ID.</param>
    /// <param name="request">The customer update request.</param>
    /// <returns>The updated customer.</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> Put(Guid id, [FromBody] UpdateCustomerRequest request)
    {
        try
        {
            var validationResult = await _updateValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(ApiResponse<CustomerDto>.Failure(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))));
            }

            var existingCustomer = await _repository.GetByIdAsync(id);
            if (existingCustomer == null)
            {
                return NotFound(ApiResponse<CustomerDto>.Failure("Customer not found."));
            }

            _mapper.Map(request, existingCustomer);
            if (TryGetSingleCompanyId(out var companyId, out _))
            {
                existingCustomer.CompanyId = companyId;
            }
            await _repository.UpdateAsync(existingCustomer);
            var dto = _mapper.Map<CustomerDto>(existingCustomer);
            return Ok(ApiResponse<CustomerDto>.Success(dto));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<CustomerDto>.Failure("An error occurred while updating the customer."));
        }
    }

    /// <summary>
    /// Deletes a customer by its ID.
    /// </summary>
    /// <param name="id">The customer ID.</param>
    /// <returns>Success or failure response.</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        try
        {
            var customer = await _repository.GetByIdAsync(id);
            if (customer == null)
            {
                return NotFound(ApiResponse<object>.Failure("Customer not found."));
            }

            await _repository.DeleteAsync(customer);
            return Ok(ApiResponse<object>.Success(null));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Failure("An error occurred while deleting the customer."));
        }
    }

    private bool TryGetSingleCompanyId(out Guid companyId, out string error)
    {
        companyId = Guid.Empty;

        if (_currentUser.CompanyIds.Count == 0)
        {
            error = "No company membership found for the current user.";
            return false;
        }

        if (_currentUser.CompanyIds.Count > 1)
        {
            error = "Multiple company memberships detected. Company selection is not implemented yet.";
            return false;
        }

        companyId = _currentUser.CompanyIds[0];
        error = string.Empty;
        return true;
    }
}