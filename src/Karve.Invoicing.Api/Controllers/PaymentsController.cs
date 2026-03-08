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
public class PaymentsController : ControllerBase
{
    private readonly IPaymentRepository _repository;
    private readonly IMapper _mapper;
    private readonly IValidator<CreatePaymentRequest> _createValidator;
    private readonly IValidator<UpdatePaymentRequest> _updateValidator;

    public PaymentsController(
        IPaymentRepository repository,
        IMapper mapper,
        IValidator<CreatePaymentRequest> createValidator,
        IValidator<UpdatePaymentRequest> updateValidator)
    {
        _repository = repository;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<PaymentDto>>>> Get(Guid companyId, int page = 1, int pageSize = 20, string? filter = null)
    {
        try
        {
            var result = await _repository.GetPagedAsync(companyId, page, pageSize, filter);
            var dtos = _mapper.Map<IEnumerable<PaymentDto>>(result.Items);
            var pagedDto = new PagedResult<PaymentDto>
            {
                Items = dtos,
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize
            };
            return Ok(ApiResponse<PagedResult<PaymentDto>>.Success(pagedDto));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<PaymentDto>>.Failure("An error occurred while retrieving payments."));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> Get(Guid id)
    {
        try
        {
            var payment = await _repository.GetByIdAsync(id);
            if (payment == null)
            {
                return NotFound(ApiResponse<PaymentDto>.Failure("Payment not found."));
            }

            var dto = _mapper.Map<PaymentDto>(payment);
            return Ok(ApiResponse<PaymentDto>.Success(dto));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PaymentDto>.Failure("An error occurred while retrieving the payment."));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> Post([FromBody] CreatePaymentRequest request)
    {
        try
        {
            var validationResult = await _createValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(ApiResponse<PaymentDto>.Failure(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))));
            }

            var payment = _mapper.Map<Payment>(request);
            await _repository.AddAsync(payment);
            var dto = _mapper.Map<PaymentDto>(payment);
            return CreatedAtAction(nameof(Get), new { id = payment.Id }, ApiResponse<PaymentDto>.Success(dto));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PaymentDto>.Failure("An error occurred while creating the payment."));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> Put(Guid id, [FromBody] UpdatePaymentRequest request)
    {
        try
        {
            var validationResult = await _updateValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(ApiResponse<PaymentDto>.Failure(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))));
            }

            var existingPayment = await _repository.GetByIdAsync(id);
            if (existingPayment == null)
            {
                return NotFound(ApiResponse<PaymentDto>.Failure("Payment not found."));
            }

            _mapper.Map(request, existingPayment);
            await _repository.UpdateAsync(existingPayment);
            var dto = _mapper.Map<PaymentDto>(existingPayment);
            return Ok(ApiResponse<PaymentDto>.Success(dto));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PaymentDto>.Failure("An error occurred while updating the payment."));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        try
        {
            var payment = await _repository.GetByIdAsync(id);
            if (payment == null)
            {
                return NotFound(ApiResponse<object>.Failure("Payment not found."));
            }

            await _repository.DeleteAsync(payment);
            return Ok(ApiResponse<object>.Success(null));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Failure("An error occurred while deleting the payment."));
        }
    }
}