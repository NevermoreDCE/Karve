using System.Net;
using System.Net.Http.Json;
using Karve.Invoicing.Application.DTOs;
using Karve.Invoicing.Application.Responses;
using Karve.Invoicing.Api;
using Karve.Invoicing.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Karve.Invoicing.Api.Tests.Controllers;

public class InvoicesControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public InvoicesControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetInvoices_ReturnsEmptyList_WhenNoInvoicesExist()
    {
        var companyId = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/invoices?companyId={companyId}&page=1&pageSize=20");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<InvoiceDto>>>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data.Items);
    }

    [Fact]
    public async Task PostInvoice_CreatesInvoice_ReturnsCreated()
    {
        var companyId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var request = new CreateInvoiceRequest
        {
            CompanyId = companyId,
            CustomerId = customerId,
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            Status = InvoiceStatus.Draft
        };
        var response = await _client.PostAsJsonAsync("/api/invoices", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<InvoiceDto>>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(request.CompanyId, result.Data.CompanyId);
        Assert.Equal(request.CustomerId, result.Data.CustomerId);
    }
}
