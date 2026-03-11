namespace Karve.Invoicing.Application.DTOs;

/// <summary>
/// Data transfer object for Company.
/// </summary>
public class CompanyDto
{
    /// <summary>
    /// The unique identifier of the company.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The name of the company.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Request model for creating a company.
/// </summary>
public class CreateCompanyRequest
{
    /// <summary>
    /// The name of the company to create.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Request model for updating a company.
/// </summary>
public class UpdateCompanyRequest
{
    /// <summary>
    /// The updated name of the company.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}