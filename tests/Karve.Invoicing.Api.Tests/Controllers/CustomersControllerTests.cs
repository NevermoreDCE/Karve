using System.Net;
using System.Net.Http.Json;
using Karve.Invoicing.Application.DTOs;
using Karve.Invoicing.Application.Responses;
using Karve.Invoicing.Api;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Karve.Invoicing.Api.Tests.Controllers;

public class CustomersControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public CustomersControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCustomers_ReturnsEmptyList_WhenNoCustomersExist()
    {
        var companyId = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/customers?companyId={companyId}&page=1&pageSize=20");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<CustomerDto>>>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data.Items);
    }

    [Fact]
    public async Task PostCustomer_CreatesCustomer_ReturnsCreated()
    {
        var companyId = Guid.NewGuid();
        var request = new CreateCustomerRequest
        {
            CompanyId = companyId,
            Name = "John Doe",
            Email = "john@example.com",
            BillingAddress = "123 Main St"
        };
        var response = await _client.PostAsJsonAsync("/api/customers", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<CustomerDto>>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(request.Name, result.Data.Name);
    }
}
