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
/// Controller for managing payments.
/// </summary>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentRepository _repository;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<CreatePaymentRequest> _createValidator;
    private readonly IValidator<UpdatePaymentRequest> _updateValidator;

    /// <summary>
    /// Initializes a new instance of the <see cref="PaymentsController"/> class.
    /// </summary>
    /// <param name="repository">The payment repository.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="createValidator">Validator for creating payments.</param>
    /// <param name="updateValidator">Validator for updating payments.</param>
    public PaymentsController(
        IPaymentRepository repository,
        IMapper mapper,
        ICurrentUserService currentUser,
        IValidator<CreatePaymentRequest> createValidator,
        IValidator<UpdatePaymentRequest> updateValidator)
    {
        _repository = repository;
        _mapper = mapper;
        _currentUser = currentUser;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    /// <summary>
    /// Gets a paginated list of payments for a specific company.
    /// </summary>
    /// <param name="page">The page number (default: 1).</param>
    /// <param name="pageSize">The number of items per page (default: 20).</param>
    /// <param name="filter">Optional filter for payment method.</param>
    /// <returns>A paginated list of payments.</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<PaymentDto>>>> Get(int page = 1, int pageSize = 20, string? filter = null)
    {
        try
        {
            if (!TryGetSingleCompanyId(out var companyId, out var contextError))
            {
                return BadRequest(ApiResponse<PagedResult<PaymentDto>>.Failure(contextError));
            }

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

    /// <summary>
    /// Gets a payment by its ID.
    /// </summary>
    /// <param name="id">The payment ID.</param>
    /// <returns>The payment details.</returns>
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

    /// <summary>
    /// Creates a new payment.
    /// </summary>
    /// <param name="request">The payment creation request.</param>
    /// <returns>The created payment.</returns>
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
            if (!TryGetSingleCompanyId(out var companyId, out var contextError))
            {
                return BadRequest(ApiResponse<PaymentDto>.Failure(contextError));
            }

            payment.CompanyId = companyId;
            await _repository.AddAsync(payment);
            var dto = _mapper.Map<PaymentDto>(payment);
            return CreatedAtAction(nameof(Get), new { id = payment.Id }, ApiResponse<PaymentDto>.Success(dto));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PaymentDto>.Failure("An error occurred while creating the payment."));
        }
    }

    /// <summary>
    /// Updates an existing payment.
    /// </summary>
    /// <param name="id">The payment ID.</param>
    /// <param name="request">The payment update request.</param>
    /// <returns>The updated payment.</returns>
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
            if (TryGetSingleCompanyId(out var companyId, out _))
            {
                existingPayment.CompanyId = companyId;
            }
            await _repository.UpdateAsync(existingPayment);
            var dto = _mapper.Map<PaymentDto>(existingPayment);
            return Ok(ApiResponse<PaymentDto>.Success(dto));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PaymentDto>.Failure("An error occurred while updating the payment."));
        }
    }

    /// <summary>
    /// Deletes a payment by its ID.
    /// </summary>
    /// <param name="id">The payment ID.</param>
    /// <returns>Success or failure response.</returns>
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