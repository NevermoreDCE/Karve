using System.Net;
using System.Net.Http.Json;
using Karve.Invoicing.Application.DTOs;
using Karve.Invoicing.Application.Responses;
using Karve.Invoicing.Api;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Karve.Invoicing.Api.Tests.Controllers;

public class CompaniesControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public CompaniesControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCompanies_ReturnsEmptyList_WhenNoCompaniesExist()
    {
        var response = await _client.GetAsync("/api/companies?page=1&pageSize=20");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<CompanyDto>>>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data.Items);
    }

    [Fact]
    public async Task PostCompany_CreatesCompany_ReturnsCreated()
    {
        var request = new CreateCompanyRequest { Name = "Test Company" };
        var response = await _client.PostAsJsonAsync("/api/companies", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<CompanyDto>>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(request.Name, result.Data.Name);
    }
}
