namespace Karve.Invoicing.Application.DTOs;

public class UserDto
{
    public Guid Id { get; set; }
    public string ExternalUserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Guid CompanyId { get; set; }
    public string Role { get; set; } = string.Empty;
}

public class CreateUserRequest
{
    public string ExternalUserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Guid CompanyId { get; set; }
    public string Role { get; set; } = string.Empty;
}

public class UpdateUserRequest
{
    public string ExternalUserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Guid CompanyId { get; set; }
    public string Role { get; set; } = string.Empty;
}