using AutoMapper;
using FluentValidation;
using Karve.Invoicing.Application.DTOs;
using Karve.Invoicing.Application.Interfaces;
using Karve.Invoicing.Application.Responses;
using Karve.Invoicing.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Karve.Invoicing.Api.Controllers;

/// <summary>
/// Controller for managing users.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _repository;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateUserRequest> _createValidator;
    private readonly IValidator<UpdateUserRequest> _updateValidator;

    /// <summary>
    /// Initializes a new instance of the <see cref="UsersController"/> class.
    /// </summary>
    /// <param name="repository">The user repository.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="createValidator">Validator for creating users.</param>
    /// <param name="updateValidator">Validator for updating users.</param>
    public UsersController(
        IUserRepository repository,
        IMapper mapper,
        IValidator<CreateUserRequest> createValidator,
        IValidator<UpdateUserRequest> updateValidator)
    {
        _repository = repository;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    /// <summary>
    /// Gets a paginated list of users for a specific company.
    /// </summary>
    /// <param name="companyId">The company ID to filter users.</param>
    /// <param name="page">The page number (default: 1).</param>
    /// <param name="pageSize">The number of items per page (default: 20).</param>
    /// <param name="filter">Optional filter for user name or email.</param>
    /// <returns>A paginated list of users.</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<UserDto>>>> Get(Guid companyId, int page = 1, int pageSize = 20, string? filter = null)
    {
        try
        {
            var result = await _repository.GetPagedAsync(companyId, page, pageSize, filter);
            var dtos = _mapper.Map<IEnumerable<UserDto>>(result.Items);
            var pagedDto = new PagedResult<UserDto>
            {
                Items = dtos,
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize
            };
            return Ok(ApiResponse<PagedResult<UserDto>>.Success(pagedDto));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<UserDto>>.Failure("An error occurred while retrieving users."));
        }
    }

    /// <summary>
    /// Gets a user by its ID.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <returns>The user details.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Get(Guid id)
    {
        try
        {
            var user = await _repository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(ApiResponse<UserDto>.Failure("User not found."));
            }

            var dto = _mapper.Map<UserDto>(user);
            return Ok(ApiResponse<UserDto>.Success(dto));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<UserDto>.Failure("An error occurred while retrieving the user."));
        }
    }

    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="request">The user creation request.</param>
    /// <returns>The created user.</returns>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<UserDto>>> Post([FromBody] CreateUserRequest request)
    {
        try
        {
            var validationResult = await _createValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(ApiResponse<UserDto>.Failure(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))));
            }

            var user = _mapper.Map<AppUser>(request);
            await _repository.AddAsync(user);
            var dto = _mapper.Map<UserDto>(user);
            return CreatedAtAction(nameof(Get), new { id = user.Id }, ApiResponse<UserDto>.Success(dto));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<UserDto>.Failure("An error occurred while creating the user."));
        }
    }

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <param name="request">The user update request.</param>
    /// <returns>The updated user.</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Put(Guid id, [FromBody] UpdateUserRequest request)
    {
        try
        {
            var validationResult = await _updateValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(ApiResponse<UserDto>.Failure(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))));
            }

            var existingUser = await _repository.GetByIdAsync(id);
            if (existingUser == null)
            {
                return NotFound(ApiResponse<UserDto>.Failure("User not found."));
            }

            _mapper.Map(request, existingUser);
            await _repository.UpdateAsync(existingUser);
            var dto = _mapper.Map<UserDto>(existingUser);
            return Ok(ApiResponse<UserDto>.Success(dto));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<UserDto>.Failure("An error occurred while updating the user."));
        }
    }

    /// <summary>
    /// Deletes a user by its ID.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <returns>Success or failure response.</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        try
        {
            var user = await _repository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(ApiResponse<object>.Failure("User not found."));
            }

            await _repository.DeleteAsync(user);
            return Ok(ApiResponse<object>.Success(null));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Failure("An error occurred while deleting the user."));
        }
    }
}