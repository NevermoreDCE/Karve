using System.Net;
using System.Net.Http.Json;
using Karve.Invoicing.Application.DTOs;
using Karve.Invoicing.Application.Responses;
using Karve.Invoicing.Domain.Entities;
using Karve.Invoicing.Domain.Enums;
using Karve.Invoicing.Domain.ValueObjects;
using Karve.Invoicing.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Karve.Invoicing.Api.Tests.Integration;

public class AuthenticationTenancyTests
{
    [Fact]
    public async Task AuthenticatedRequest_ProvisionsLocalUser()
    {
        using var factory = new IntegrationWebApplicationFactory();
        await ResetDatabaseAsync(factory);

        var externalUserId = Guid.NewGuid().ToString();
        var client = factory.CreateClient();
        AddAuthHeaders(client, externalUserId, "provisioned@example.com");

        // User has no company assignment yet, so middleware should still provision then return 403.
        var response = await client.GetAsync("/api/customers");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<InvoicingDbContext>();
        var user = db.Users.FirstOrDefault(u => u.ExternalUserId == externalUserId);

        Assert.NotNull(user);
        Assert.Equal("provisioned@example.com", user!.Email.Value);
    }

    [Fact]
    public async Task UserWithoutCompanyMembership_ReturnsForbidden()
    {
        using var factory = new IntegrationWebApplicationFactory();
        await ResetDatabaseAsync(factory);

        var externalUserId = Guid.NewGuid().ToString();
        await SeedUserWithoutMembershipAsync(factory, externalUserId, "nomembership@example.com");

        var client = factory.CreateClient();
        AddAuthHeaders(client, externalUserId, "nomembership@example.com");

        var response = await client.GetAsync("/api/customers");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AllowedUser_GetsOnlyTenantScopedCustomers()
    {
        using var factory = new IntegrationWebApplicationFactory();
        await ResetDatabaseAsync(factory);

        var externalUserId = Guid.NewGuid().ToString();
        var expectedCompanyId = await SeedTwoCompaniesWithMembershipAndCustomersAsync(factory, externalUserId);

        var client = factory.CreateClient();
        AddAuthHeaders(client, externalUserId, "allowed@example.com");

        var response = await client.GetAsync("/api/customers");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<CustomerDto>>>();
        Assert.NotNull(payload);
        Assert.True(payload!.IsSuccess);
        Assert.NotNull(payload.Data);
        Assert.NotEmpty(payload.Data!.Items);
        Assert.All(payload.Data.Items, c => Assert.Equal(expectedCompanyId, c.CompanyId));
    }

    private static void AddAuthHeaders(HttpClient client, string externalUserId, string email)
    {
        client.DefaultRequestHeaders.Remove(TestAuthHandler.OidHeader);
        client.DefaultRequestHeaders.Remove(TestAuthHandler.EmailHeader);
        client.DefaultRequestHeaders.Add(TestAuthHandler.OidHeader, externalUserId);
        client.DefaultRequestHeaders.Add(TestAuthHandler.EmailHeader, email);
    }

    private static async Task ResetDatabaseAsync(IntegrationWebApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<InvoicingDbContext>();

        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }

    private static async Task SeedUserWithoutMembershipAsync(IntegrationWebApplicationFactory factory, string externalUserId, string email)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<InvoicingDbContext>();

        db.Users.Add(new AppUser
        {
            Id = Guid.NewGuid(),
            ExternalUserId = externalUserId,
            Email = new EmailAddress(email)
        });

        await db.SaveChangesAsync();
    }

    private static async Task<Guid> SeedTwoCompaniesWithMembershipAndCustomersAsync(IntegrationWebApplicationFactory factory, string externalUserId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<InvoicingDbContext>();

        var allowedCompany = new Company { Id = Guid.NewGuid(), Name = "Allowed Co" };
        var otherCompany = new Company { Id = Guid.NewGuid(), Name = "Other Co" };

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            ExternalUserId = externalUserId,
            Email = new EmailAddress("allowed@example.com")
        };

        db.Companies.AddRange(allowedCompany, otherCompany);
        db.Users.Add(user);
        db.CompanyUsers.Add(new CompanyUser
        {
            CompanyId = allowedCompany.Id,
            UserId = user.Id,
            Role = UserRole.Admin
        });

        db.Customers.AddRange(
            new Customer
            {
                Id = Guid.NewGuid(),
                CompanyId = allowedCompany.Id,
                Name = "Tenant Customer",
                Email = new EmailAddress("tenant.customer@example.com"),
                BillingAddress = "100 Tenant Way"
            },
            new Customer
            {
                Id = Guid.NewGuid(),
                CompanyId = otherCompany.Id,
                Name = "Other Customer",
                Email = new EmailAddress("other.customer@example.com"),
                BillingAddress = "999 Other Ave"
            });

        await db.SaveChangesAsync();
        return allowedCompany.Id;
    }
}
