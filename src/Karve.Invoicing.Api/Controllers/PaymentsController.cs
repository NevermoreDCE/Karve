using AutoMapper;
using FluentValidation;
using Karve.Invoicing.Api.Logging;
using Karve.Invoicing.Application.DTOs;
using Karve.Invoicing.Application.Interfaces;
using Karve.Invoicing.Application.Responses;
using Karve.Invoicing.Application.Services;
using Karve.Invoicing.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics;
using Karve.Invoicing.Api.Observability;

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
    private readonly ILogger<PaymentsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PaymentsController"/> class.
    /// </summary>
    /// <param name="repository">The payment repository.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="currentUser">Current authenticated user context.</param>
    /// <param name="createValidator">Validator for creating payments.</param>
    /// <param name="updateValidator">Validator for updating payments.</param>
    /// <param name="logger">Logger for controller diagnostics.</param>
    public PaymentsController(
        IPaymentRepository repository,
        IMapper mapper,
        ICurrentUserService currentUser,
        IValidator<CreatePaymentRequest> createValidator,
        IValidator<UpdatePaymentRequest> updateValidator,
        ILogger<PaymentsController>? logger = null)
    {
        _repository = repository;
        _mapper = mapper;
        _currentUser = currentUser;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger ?? NullLogger<PaymentsController>.Instance;
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
            _logger.LogError(
                ex,
                "Failed to retrieve payments. Page={Page} PageSize={PageSize} Filter={Filter} CompanyMembershipCount={CompanyMembershipCount}",
                page,
                pageSize,
                LogSanitizer.SanitizeForLog(filter),
                _currentUser.CompanyIds.Count);

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
            _logger.LogError(
                ex,
                "Failed to retrieve payment by id. PaymentId={PaymentId}",
                id);

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
        using var activity = KarveActivitySource.Instance.StartActivity("payment.process", ActivityKind.Internal);
        activity?.SetTag("payment.invoice_id", request.InvoiceId);
        activity?.SetTag("payment.method", request.Method.ToString());
        activity?.SetTag("payment.amount", request.Amount);
        activity?.SetTag("payment.currency", request.Currency);

        try
        {
            var validationResult = await _createValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                activity?.SetStatus(ActivityStatusCode.Error, "Payment validation failed.");
                return BadRequest(ApiResponse<PaymentDto>.Failure(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))));
            }

            var payment = _mapper.Map<Payment>(request);
            if (!TryGetSingleCompanyId(out var companyId, out var contextError))
            {
                return BadRequest(ApiResponse<PaymentDto>.Failure(contextError));
            }

            payment.CompanyId = companyId;
            await _repository.AddAsync(payment);

            activity?.SetTag("payment.id", payment.Id);
            activity?.SetTag("payment.company_id", payment.CompanyId);
            activity?.SetStatus(ActivityStatusCode.Ok);

            var dto = _mapper.Map<PaymentDto>(payment);
            return CreatedAtAction(nameof(Get), new { id = payment.Id }, ApiResponse<PaymentDto>.Success(dto));
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddEvent(new ActivityEvent(
                "exception",
                tags: new ActivityTagsCollection
                {
                    ["exception.type"] = ex.GetType().FullName,
                    ["exception.message"] = ex.Message,
                    ["exception.stacktrace"] = ex.StackTrace
                }));

            _logger.LogError(
                ex,
                "Failed to create payment. InvoiceId={InvoiceId} Amount={Amount} Currency={Currency} PaymentDate={PaymentDate} Method={Method}",
                request.InvoiceId,
                request.Amount,
                LogSanitizer.SanitizeForLog(request.Currency),
                request.PaymentDate,
                request.Method);

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
            _logger.LogError(
                ex,
                "Failed to update payment. PaymentId={PaymentId} InvoiceId={InvoiceId} Amount={Amount} Currency={Currency} PaymentDate={PaymentDate} Method={Method}",
                id,
                request.InvoiceId,
                request.Amount,
                LogSanitizer.SanitizeForLog(request.Currency),
                request.PaymentDate,
                request.Method);

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
            _logger.LogError(
                ex,
                "Failed to delete payment. PaymentId={PaymentId}",
                id);

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