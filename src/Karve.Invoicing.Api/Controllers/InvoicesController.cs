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
/// Controller for managing invoices.
/// </summary>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceRepository _repository;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<CreateInvoiceRequest> _createValidator;
    private readonly IValidator<UpdateInvoiceRequest> _updateValidator;

    /// <summary>
    /// Initializes a new instance of the <see cref="InvoicesController"/> class.
    /// </summary>
    /// <param name="repository">The invoice repository.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="createValidator">Validator for creating invoices.</param>
    /// <param name="updateValidator">Validator for updating invoices.</param>
    public InvoicesController(
        IInvoiceRepository repository,
        IMapper mapper,
        ICurrentUserService currentUser,
        IValidator<CreateInvoiceRequest> createValidator,
        IValidator<UpdateInvoiceRequest> updateValidator)
    {
        _repository = repository;
        _mapper = mapper;
        _currentUser = currentUser;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    /// <summary>
    /// Gets a paginated list of invoices for a specific company.
    /// </summary>
    /// <param name="page">The page number (default: 1).</param>
    /// <param name="pageSize">The number of items per page (default: 20).</param>
    /// <param name="filter">Optional filter for invoice status.</param>
    /// <returns>A paginated list of invoices.</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<InvoiceDto>>>> Get(int page = 1, int pageSize = 20, string? filter = null)
    {
        try
        {
            if (!TryGetSingleCompanyId(out var companyId, out var contextError))
            {
                return BadRequest(ApiResponse<PagedResult<InvoiceDto>>.Failure(contextError));
            }

            var result = await _repository.GetPagedAsync(companyId, page, pageSize, filter);
            var dtos = _mapper.Map<IEnumerable<InvoiceDto>>(result.Items);
            var pagedDto = new PagedResult<InvoiceDto>
            {
                Items = dtos,
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize
            };
            return Ok(ApiResponse<PagedResult<InvoiceDto>>.Success(pagedDto));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<InvoiceDto>>.Failure("An error occurred while retrieving invoices."));
        }
    }

    /// <summary>
    /// Gets an invoice by its ID.
    /// </summary>
    /// <param name="id">The invoice ID.</param>
    /// <returns>The invoice details.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<InvoiceDto>>> Get(Guid id)
    {
        try
        {
            var invoice = await _repository.GetByIdAsync(id);
            if (invoice == null)
            {
                return NotFound(ApiResponse<InvoiceDto>.Failure("Invoice not found."));
            }

            var dto = _mapper.Map<InvoiceDto>(invoice);
            return Ok(ApiResponse<InvoiceDto>.Success(dto));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<InvoiceDto>.Failure("An error occurred while retrieving the invoice."));
        }
    }

    /// <summary>
    /// Creates a new invoice.
    /// </summary>
    /// <param name="request">The invoice creation request.</param>
    /// <returns>The created invoice.</returns>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<InvoiceDto>>> Post([FromBody] CreateInvoiceRequest request)
    {
        try
        {
            var validationResult = await _createValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(ApiResponse<InvoiceDto>.Failure(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))));
            }

            var invoice = _mapper.Map<Invoice>(request);
            if (!TryGetSingleCompanyId(out var companyId, out var contextError))
            {
                return BadRequest(ApiResponse<InvoiceDto>.Failure(contextError));
            }

            invoice.CompanyId = companyId;
            await _repository.AddAsync(invoice);
            var dto = _mapper.Map<InvoiceDto>(invoice);
            return CreatedAtAction(nameof(Get), new { id = invoice.Id }, ApiResponse<InvoiceDto>.Success(dto));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<InvoiceDto>.Failure("An error occurred while creating the invoice."));
        }
    }

    /// <summary>
    /// Updates an existing invoice.
    /// </summary>
    /// <param name="id">The invoice ID.</param>
    /// <param name="request">The invoice update request.</param>
    /// <returns>The updated invoice.</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<InvoiceDto>>> Put(Guid id, [FromBody] UpdateInvoiceRequest request)
    {
        try
        {
            var validationResult = await _updateValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(ApiResponse<InvoiceDto>.Failure(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))));
            }

            var existingInvoice = await _repository.GetByIdAsync(id);
            if (existingInvoice == null)
            {
                return NotFound(ApiResponse<InvoiceDto>.Failure("Invoice not found."));
            }

            _mapper.Map(request, existingInvoice);
            if (TryGetSingleCompanyId(out var companyId, out _))
            {
                existingInvoice.CompanyId = companyId;
            }
            await _repository.UpdateAsync(existingInvoice);
            var dto = _mapper.Map<InvoiceDto>(existingInvoice);
            return Ok(ApiResponse<InvoiceDto>.Success(dto));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<InvoiceDto>.Failure("An error occurred while updating the invoice."));
        }
    }

    /// <summary>
    /// Deletes an invoice by its ID.
    /// </summary>
    /// <param name="id">The invoice ID.</param>
    /// <returns>Success or failure response.</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        try
        {
            var invoice = await _repository.GetByIdAsync(id);
            if (invoice == null)
            {
                return NotFound(ApiResponse<object>.Failure("Invoice not found."));
            }

            await _repository.DeleteAsync(invoice);
            return Ok(ApiResponse<object>.Success(null));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Failure("An error occurred while deleting the invoice."));
        }
    }

    // Additional endpoints for line items
    /// <summary>
    /// Gets line items for a specific invoice.
    /// </summary>
    /// <param name="id">The invoice ID.</param>
    /// <returns>The invoice line items.</returns>
    [HttpGet("{id}/line-items")]
    public async Task<ActionResult<ApiResponse<IEnumerable<InvoiceLineItemDto>>>> GetLineItems(Guid id)
    {
        try
        {
            var invoice = await _repository.GetByIdAsync(id);
            if (invoice == null)
            {
                return NotFound(ApiResponse<IEnumerable<InvoiceLineItemDto>>.Failure("Invoice not found."));
            }

            var dtos = _mapper.Map<IEnumerable<InvoiceLineItemDto>>(invoice.LineItems);
            return Ok(ApiResponse<IEnumerable<InvoiceLineItemDto>>.Success(dtos));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<IEnumerable<InvoiceLineItemDto>>.Failure("An error occurred while retrieving line items."));
        }
    }

    /// <summary>
    /// Adds a line item to a specific invoice.
    /// </summary>
    /// <param name="id">The invoice ID.</param>
    /// <param name="request">The line item creation request.</param>
    /// <returns>The created line item.</returns>
    [HttpPost("{id}/line-items")]
    public async Task<ActionResult<ApiResponse<InvoiceLineItemDto>>> PostLineItem(Guid id, [FromBody] CreateInvoiceLineItemRequest request)
    {
        // Placeholder - would need InvoiceLineItemRepository
        return StatusCode(501, ApiResponse<InvoiceLineItemDto>.Failure("Not implemented yet."));
    }

    // Additional endpoints for payments
    /// <summary>
    /// Gets payments for a specific invoice.
    /// </summary>
    /// <param name="id">The invoice ID.</param>
    /// <returns>The payments applied to the invoice.</returns>
    [HttpGet("{id}/payments")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PaymentDto>>>> GetPayments(Guid id)
    {
        try
        {
            var invoice = await _repository.GetByIdAsync(id);
            if (invoice == null)
            {
                return NotFound(ApiResponse<IEnumerable<PaymentDto>>.Failure("Invoice not found."));
            }

            var dtos = _mapper.Map<IEnumerable<PaymentDto>>(invoice.Payments);
            return Ok(ApiResponse<IEnumerable<PaymentDto>>.Success(dtos));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<IEnumerable<PaymentDto>>.Failure("An error occurred while retrieving payments."));
        }
    }

    /// <summary>
    /// Adds a payment to a specific invoice.
    /// </summary>
    /// <param name="id">The invoice ID.</param>
    /// <param name="request">The payment creation request.</param>
    /// <returns>The created payment.</returns>
    [HttpPost("{id}/payments")]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> PostPayment(Guid id, [FromBody] CreatePaymentRequest request)
    {
        // Placeholder - would need PaymentRepository
        return StatusCode(501, ApiResponse<PaymentDto>.Failure("Not implemented yet."));
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