namespace Karve.Invoicing.Application.DTOs;

public class ProductDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal UnitPriceAmount { get; set; }
    public string UnitPriceCurrency { get; set; } = "USD";
}

public class CreateProductRequest
{
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal UnitPriceAmount { get; set; }
    public string UnitPriceCurrency { get; set; } = "USD";
}

public class UpdateProductRequest
{
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal UnitPriceAmount { get; set; }
    public string UnitPriceCurrency { get; set; } = "USD";
}