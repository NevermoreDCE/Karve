using Karve.Invoicing.Api.Middleware;

namespace Karve.Invoicing.Api.Tests.Integration;

public class CorrelationIdIntegrationTests : IClassFixture<IntegrationWebApplicationFactory>
{
    private readonly IntegrationWebApplicationFactory _factory;

    public CorrelationIdIntegrationTests(IntegrationWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Response_WithoutCorrelationIdHeader_IncludesGeneratedCorrelationId()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/invoices");

        var hasHeader = response.Headers.TryGetValues(CorrelationIdMiddleware.HeaderName, out var values);
        Assert.True(hasHeader, "Response should include the X-Correlation-ID header.");
        Assert.NotEmpty(values!.First());
    }

    [Fact]
    public async Task Response_WithCorrelationIdHeader_EchoesProvidedCorrelationId()
    {
        const string correlationId = "integration-test-corr-id-xyz789";
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(CorrelationIdMiddleware.HeaderName, correlationId);

        var response = await client.GetAsync("/api/invoices");

        var hasHeader = response.Headers.TryGetValues(CorrelationIdMiddleware.HeaderName, out var values);
        Assert.True(hasHeader, "Response should include the X-Correlation-ID header.");
        Assert.Equal(correlationId, values!.First());
    }
}
