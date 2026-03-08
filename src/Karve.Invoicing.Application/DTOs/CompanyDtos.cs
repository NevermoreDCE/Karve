namespace Karve.Invoicing.Application.DTOs;

public class CompanyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class CreateCompanyRequest
{
    public string Name { get; set; } = string.Empty;
}

public class UpdateCompanyRequest
{
    public string Name { get; set; } = string.Empty;
}