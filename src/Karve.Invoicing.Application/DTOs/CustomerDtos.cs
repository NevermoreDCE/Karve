namespace Karve.Invoicing.Application.DTOs;

public class CustomerDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string BillingAddress { get; set; } = string.Empty;
}

public class CreateCustomerRequest
{
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string BillingAddress { get; set; } = string.Empty;
}

public class UpdateCustomerRequest
{
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string BillingAddress { get; set; } = string.Empty;
}