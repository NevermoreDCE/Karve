using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Karve.Invoicing.Api.Tests.Integration;

public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Test";
    public const string OidHeader = "X-Test-Oid";
    public const string EmailHeader = "X-Test-Email";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(OidHeader, out var oidHeader) || string.IsNullOrWhiteSpace(oidHeader))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var oid = oidHeader.ToString();
        var email = Request.Headers.TryGetValue(EmailHeader, out var emailHeader)
            ? emailHeader.ToString()
            : "test.user@example.com";

        var claims = new[]
        {
            new Claim("oid", oid),
            new Claim("preferred_username", email),
            new Claim(ClaimTypes.NameIdentifier, oid),
            new Claim(ClaimTypes.Email, email)
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
