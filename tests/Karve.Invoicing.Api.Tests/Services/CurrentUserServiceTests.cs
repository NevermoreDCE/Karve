using System.Security.Claims;
using Karve.Invoicing.Api.Middleware;
using Karve.Invoicing.Api.Services;
using Microsoft.AspNetCore.Http;

namespace Karve.Invoicing.Api.Tests.Services;

public class CurrentUserServiceTests
{
    [Fact]
    public void ReadsUserIdAndEmail_FromClaims()
    {
        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = BuildHttpContext(
                oid: "11111111-1111-1111-1111-111111111111",
                email: "claims@example.com")
        };

        var service = new CurrentUserService(httpContextAccessor);

        Assert.Equal("11111111-1111-1111-1111-111111111111", service.UserId);
        Assert.Equal("claims@example.com", service.Email);
    }

    [Fact]
    public void CompanyIds_ReadFromResolvedTenantContextItems()
    {
        var expectedCompanyId = Guid.NewGuid();

        var context = BuildHttpContext(
            oid: Guid.NewGuid().ToString(),
            email: "local-id@example.com");
        context.Items[TenantResolutionMiddleware.ResolvedCompanyIdsItemKey] = new List<Guid> { expectedCompanyId };

        var httpContextAccessor = new HttpContextAccessor { HttpContext = context };

        var service = new CurrentUserService(httpContextAccessor);

        Assert.Single(service.CompanyIds);
        Assert.Equal(expectedCompanyId, service.CompanyIds[0]);
    }

    private static HttpContext BuildHttpContext(string oid, string email)
    {
        var claims = new[]
        {
            new Claim("oid", oid),
            new Claim("preferred_username", email),
            new Claim(ClaimTypes.NameIdentifier, oid),
            new Claim(ClaimTypes.Email, email)
        };

        var identity = new ClaimsIdentity(claims, "Test");
        return new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identity)
        };
    }
}
