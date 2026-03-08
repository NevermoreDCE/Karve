using AutoMapper;
using FluentValidation;
using Karve.Invoicing.Application.DTOs;
using Karve.Invoicing.Application.Interfaces;
using Karve.Invoicing.Application.Responses;
using Karve.Invoicing.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Karve.Invoicing.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceRepository _repository;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateInvoiceRequest> _createValidator;
    private readonly IValidator<UpdateInvoiceRequest> _updateValidator;

    public InvoicesController(
        IInvoiceRepository repository,
        IMapper mapper,
        IValidator<CreateInvoiceRequest> createValidator,
        IValidator<UpdateInvoiceRequest> updateValidator)
    {
        _repository = repository;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<InvoiceDto>>>> Get(Guid companyId, int page = 1, int pageSize = 20, string? filter = null)
    {
        try
        {
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
            await _repository.AddAsync(invoice);
            var dto = _mapper.Map<InvoiceDto>(invoice);
            return CreatedAtAction(nameof(Get), new { id = invoice.Id }, ApiResponse<InvoiceDto>.Success(dto));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<InvoiceDto>.Failure("An error occurred while creating the invoice."));
        }
    }

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
            await _repository.UpdateAsync(existingInvoice);
            var dto = _mapper.Map<InvoiceDto>(existingInvoice);
            return Ok(ApiResponse<InvoiceDto>.Success(dto));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<InvoiceDto>.Failure("An error occurred while updating the invoice."));
        }
    }

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

    [HttpPost("{id}/line-items")]
    public async Task<ActionResult<ApiResponse<InvoiceLineItemDto>>> PostLineItem(Guid id, [FromBody] CreateInvoiceLineItemRequest request)
    {
        // Placeholder - would need InvoiceLineItemRepository
        return StatusCode(501, ApiResponse<InvoiceLineItemDto>.Failure("Not implemented yet."));
    }

    // Additional endpoints for payments
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

    [HttpPost("{id}/payments")]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> PostPayment(Guid id, [FromBody] CreatePaymentRequest request)
    {
        // Placeholder - would need PaymentRepository
        return StatusCode(501, ApiResponse<PaymentDto>.Failure("Not implemented yet."));
    }
}