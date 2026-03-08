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
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _repository;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateProductRequest> _createValidator;
    private readonly IValidator<UpdateProductRequest> _updateValidator;

    public ProductsController(
        IProductRepository repository,
        IMapper mapper,
        IValidator<CreateProductRequest> createValidator,
        IValidator<UpdateProductRequest> updateValidator)
    {
        _repository = repository;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> Get()
    {
        try
        {
            var products = await _repository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<ProductDto>>(products);
            return Ok(ApiResponse<IEnumerable<ProductDto>>.Success(dtos));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<IEnumerable<ProductDto>>.Failure("An error occurred while retrieving products."));
        }
    }

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
            return StatusCode(500, ApiResponse<ProductDto>.Failure("An error occurred while retrieving the product."));
        }
    }

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
            await _repository.AddAsync(product);
            var dto = _mapper.Map<ProductDto>(product);
            return CreatedAtAction(nameof(Get), new { id = product.Id }, ApiResponse<ProductDto>.Success(dto));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<ProductDto>.Failure("An error occurred while creating the product."));
        }
    }

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
            await _repository.UpdateAsync(existingProduct);
            var dto = _mapper.Map<ProductDto>(existingProduct);
            return Ok(ApiResponse<ProductDto>.Success(dto));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<ProductDto>.Failure("An error occurred while updating the product."));
        }
    }

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
            return StatusCode(500, ApiResponse<object>.Failure("An error occurred while deleting the product."));
        }
    }
}