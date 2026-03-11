using System.Net;
using System.Net.Http.Json;
using Karve.Invoicing.Application.DTOs;
using Karve.Invoicing.Application.Responses;
using Karve.Invoicing.Api;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Karve.Invoicing.Api.Tests.Controllers;

public class ProductsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ProductsControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProducts_ReturnsEmptyList_WhenNoProductsExist()
    {
        var companyId = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/products?companyId={companyId}&page=1&pageSize=20");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<ProductDto>>>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data.Items);
    }

    [Fact]
    public async Task PostProduct_CreatesProduct_ReturnsCreated()
    {
        var companyId = Guid.NewGuid();
        var request = new CreateProductRequest
        {
            CompanyId = companyId,
            Name = "Widget",
            Sku = "WDG-001",
            UnitPriceAmount = 19.99m,
            UnitPriceCurrency = "USD"
        };
        var response = await _client.PostAsJsonAsync("/api/products", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(request.Name, result.Data.Name);
    }
}
