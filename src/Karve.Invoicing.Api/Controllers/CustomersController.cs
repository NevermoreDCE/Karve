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
public class CustomersController : ControllerBase
{
    private readonly ICustomerRepository _repository;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateCustomerRequest> _createValidator;
    private readonly IValidator<UpdateCustomerRequest> _updateValidator;

    public CustomersController(
        ICustomerRepository repository,
        IMapper mapper,
        IValidator<CreateCustomerRequest> createValidator,
        IValidator<UpdateCustomerRequest> updateValidator)
    {
        _repository = repository;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<CustomerDto>>>> Get()
    {
        try
        {
            var customers = await _repository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<CustomerDto>>(customers);
            return Ok(ApiResponse<IEnumerable<CustomerDto>>.Success(dtos));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<IEnumerable<CustomerDto>>.Failure("An error occurred while retrieving customers."));
        }
    }

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
            await _repository.AddAsync(customer);
            var dto = _mapper.Map<CustomerDto>(customer);
            return CreatedAtAction(nameof(Get), new { id = customer.Id }, ApiResponse<CustomerDto>.Success(dto));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<CustomerDto>.Failure("An error occurred while creating the customer."));
        }
    }

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
            await _repository.UpdateAsync(existingCustomer);
            var dto = _mapper.Map<CustomerDto>(existingCustomer);
            return Ok(ApiResponse<CustomerDto>.Success(dto));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<CustomerDto>.Failure("An error occurred while updating the customer."));
        }
    }

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
}