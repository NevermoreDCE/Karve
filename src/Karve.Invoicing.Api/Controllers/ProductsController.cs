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

namespace Karve.Invoicing.Api.Controllers;

/// <summary>
/// Controller for managing products.
/// </summary>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _repository;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<CreateProductRequest> _createValidator;
    private readonly IValidator<UpdateProductRequest> _updateValidator;
    private readonly ILogger<ProductsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductsController"/> class.
    /// </summary>
    /// <param name="repository">The product repository.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="currentUser">Current authenticated user context.</param>
    /// <param name="createValidator">Validator for creating products.</param>
    /// <param name="updateValidator">Validator for updating products.</param>
    /// <param name="logger">Logger for controller diagnostics.</param>
    public ProductsController(
        IProductRepository repository,
        IMapper mapper,
        ICurrentUserService currentUser,
        IValidator<CreateProductRequest> createValidator,
        IValidator<UpdateProductRequest> updateValidator,
        ILogger<ProductsController>? logger = null)
    {
        _repository = repository;
        _mapper = mapper;
        _currentUser = currentUser;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger ?? NullLogger<ProductsController>.Instance;
    }

    /// <summary>
    /// Gets a paginated list of products for a specific company.
    /// </summary>
    /// <param name="page">The page number (default: 1).</param>
    /// <param name="pageSize">The number of items per page (default: 20).</param>
    /// <param name="filter">Optional filter for product name or SKU.</param>
    /// <returns>A paginated list of products.</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<ProductDto>>>> Get(int page = 1, int pageSize = 20, string? filter = null)
    {
        try
        {
            if (!TryGetSingleCompanyId(out var companyId, out var contextError))
            {
                return BadRequest(ApiResponse<PagedResult<ProductDto>>.Failure(contextError));
            }

            var result = await _repository.GetPagedAsync(companyId, page, pageSize, filter);
            var dtos = _mapper.Map<IEnumerable<ProductDto>>(result.Items);
            var pagedDto = new PagedResult<ProductDto>
            {
                Items = dtos,
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize
            };
            return Ok(ApiResponse<PagedResult<ProductDto>>.Success(pagedDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to retrieve products. Page={Page} PageSize={PageSize} Filter={Filter} CompanyMembershipCount={CompanyMembershipCount}",
                page,
                pageSize,
                LogSanitizer.SanitizeForLog(filter),
                _currentUser.CompanyIds.Count);

            return StatusCode(500, ApiResponse<PagedResult<ProductDto>>.Failure("An error occurred while retrieving products."));
        }
    }

    /// <summary>
    /// Gets a product by its ID.
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <returns>The product details.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Get(Guid id)
    {
        try
        {
            var product = await _repository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound(ApiResponse<ProductDto>.Failure("Product not found."));
            }

            var dto = _mapper.Map<ProductDto>(product);
            return Ok(ApiResponse<ProductDto>.Success(dto));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to retrieve product by id. ProductId={ProductId}",
                id);

            return StatusCode(500, ApiResponse<ProductDto>.Failure("An error occurred while retrieving the product."));
        }
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="request">The product creation request.</param>
    /// <returns>The created product.</returns>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Post([FromBody] CreateProductRequest request)
    {
        try
        {
            var validationResult = await _createValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(ApiResponse<ProductDto>.Failure(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))));
            }

            var product = _mapper.Map<Product>(request);
            if (!TryGetSingleCompanyId(out var companyId, out var contextError))
            {
                return BadRequest(ApiResponse<ProductDto>.Failure(contextError));
            }

            product.CompanyId = companyId;
            await _repository.AddAsync(product);
            var dto = _mapper.Map<ProductDto>(product);
            return CreatedAtAction(nameof(Get), new { id = product.Id }, ApiResponse<ProductDto>.Success(dto));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to create product. Name={ProductName} Sku={Sku} UnitPriceAmount={UnitPriceAmount} UnitPriceCurrency={UnitPriceCurrency}",
                LogSanitizer.SanitizeForLog(request.Name),
                LogSanitizer.SanitizeForLog(request.Sku),
                request.UnitPriceAmount,
                LogSanitizer.SanitizeForLog(request.UnitPriceCurrency));

            return StatusCode(500, ApiResponse<ProductDto>.Failure("An error occurred while creating the product."));
        }
    }

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <param name="request">The product update request.</param>
    /// <returns>The updated product.</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Put(Guid id, [FromBody] UpdateProductRequest request)
    {
        try
        {
            var validationResult = await _updateValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(ApiResponse<ProductDto>.Failure(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))));
            }

            var existingProduct = await _repository.GetByIdAsync(id);
            if (existingProduct == null)
            {
                return NotFound(ApiResponse<ProductDto>.Failure("Product not found."));
            }

            _mapper.Map(request, existingProduct);
            if (TryGetSingleCompanyId(out var companyId, out _))
            {
                existingProduct.CompanyId = companyId;
            }
            await _repository.UpdateAsync(existingProduct);
            var dto = _mapper.Map<ProductDto>(existingProduct);
            return Ok(ApiResponse<ProductDto>.Success(dto));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to update product. ProductId={ProductId} Name={ProductName} Sku={Sku} UnitPriceAmount={UnitPriceAmount} UnitPriceCurrency={UnitPriceCurrency}",
                id,
                LogSanitizer.SanitizeForLog(request.Name),
                LogSanitizer.SanitizeForLog(request.Sku),
                request.UnitPriceAmount,
                LogSanitizer.SanitizeForLog(request.UnitPriceCurrency));

            return StatusCode(500, ApiResponse<ProductDto>.Failure("An error occurred while updating the product."));
        }
    }

    /// <summary>
    /// Deletes a product by its ID.
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <returns>Success or failure response.</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        try
        {
            var product = await _repository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound(ApiResponse<object>.Failure("Product not found."));
            }

            await _repository.DeleteAsync(product);
            return Ok(ApiResponse<object>.Success(null));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to delete product. ProductId={ProductId}",
                id);

            return StatusCode(500, ApiResponse<object>.Failure("An error occurred while deleting the product."));
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