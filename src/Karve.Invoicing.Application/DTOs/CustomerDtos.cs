namespace Karve.Invoicing.Application.DTOs;

/// <summary>
/// Data transfer object for Customer.
/// </summary>
public class CustomerDto
{
    /// <summary>
    /// The unique identifier of the customer.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The company ID the customer belongs to.
    /// </summary>
    public Guid CompanyId { get; set; }

    /// <summary>
    /// The name of the customer.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The email address of the customer.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The billing address of the customer.
    /// </summary>
    public string BillingAddress { get; set; } = string.Empty;
}

/// <summary>
/// Request model for creating a customer.
/// </summary>
public class CreateCustomerRequest
{
    /// <summary>
    /// The name of the customer.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The email address of the customer.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The billing address of the customer.
    /// </summary>
    public string BillingAddress { get; set; } = string.Empty;
}

/// <summary>
/// Request model for updating a customer.
/// </summary>
public class UpdateCustomerRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string BillingAddress { get; set; } = string.Empty;
}