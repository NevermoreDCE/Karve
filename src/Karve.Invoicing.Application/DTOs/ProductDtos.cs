namespace Karve.Invoicing.Application.DTOs;

/// <summary>
/// Data transfer object for Product.
/// </summary>
public class ProductDto
{
    /// <summary>
    /// The unique identifier of the product.
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// The company ID the product belongs to.
    /// </summary>
    public Guid CompanyId { get; set; }
    /// <summary>
    /// The name of the product.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// The SKU of the product.
    /// </summary>
    public string Sku { get; set; } = string.Empty;
    /// <summary>
    /// The unit price amount.
    /// </summary>
    public decimal UnitPriceAmount { get; set; }
    /// <summary>
    /// The unit price currency.
    /// </summary>
    public string UnitPriceCurrency { get; set; } = "USD";
}

/// <summary>
/// Request model for creating a product.
/// </summary>
public class CreateProductRequest
{
    /// <summary>
    /// The company ID the product belongs to.
    /// </summary>
    public Guid CompanyId { get; set; }
    /// <summary>
    /// The name of the product.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// The SKU of the product.
    /// </summary>
    public string Sku { get; set; } = string.Empty;
    /// <summary>
    /// The unit price amount.
    /// </summary>
    public decimal UnitPriceAmount { get; set; }
    /// <summary>
    /// The unit price currency.
    /// </summary>
    public string UnitPriceCurrency { get; set; } = "USD";
}

/// <summary>
/// Request model for updating a product.
/// </summary>
public class UpdateProductRequest
{
    /// <summary>
    /// The company ID the product belongs to.
    /// </summary>
    public Guid CompanyId { get; set; }
    /// <summary>
    /// The name of the product.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// The SKU of the product.
    /// </summary>
    public string Sku { get; set; } = string.Empty;
    /// <summary>
    /// The unit price amount.
    /// </summary>
    public decimal UnitPriceAmount { get; set; }
    /// <summary>
    /// The unit price currency.
    /// </summary>
    public string UnitPriceCurrency { get; set; } = "USD";
}