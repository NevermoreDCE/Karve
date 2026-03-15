using Karve.Invoicing.Api.Observability;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Karve.Invoicing.Api.Tests.Observability;

public class OpenTelemetryConfigurationTests
{
    // -------------------------------------------------------------------------
    // OpenTelemetryOptions — defaults and configuration binding
    // -------------------------------------------------------------------------

    [Fact]
    public void OpenTelemetryOptions_DefaultValues_AreReasonable()
    {
        var options = new OpenTelemetryOptions();

        Assert.Equal("karve-invoicing-api", options.ServiceName);
        Assert.Equal(string.Empty, options.OtlpEndpoint);
        Assert.True(options.EnableTraces);
        Assert.True(options.EnableMetrics);
        Assert.True(options.EnableLogs);
    }

    [Fact]
    public void OpenTelemetryOptions_SectionName_IsOpenTelemetry()
    {
        Assert.Equal("OpenTelemetry", OpenTelemetryOptions.SectionName);
    }

    [Fact]
    public void OpenTelemetryOptions_CanBeConfiguredViaConfigurationSection()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OpenTelemetry:ServiceName"] = "my-test-service",
                ["OpenTelemetry:OtlpEndpoint"] = "http://localhost:4317",
                ["OpenTelemetry:Environment"] = "Staging",
                ["OpenTelemetry:EnableTraces"] = "false",
                ["OpenTelemetry:EnableMetrics"] = "true",
                ["OpenTelemetry:EnableLogs"] = "false",
            })
            .Build();

        var options = config.GetSection(OpenTelemetryOptions.SectionName).Get<OpenTelemetryOptions>()!;

        Assert.Equal("my-test-service", options.ServiceName);
        Assert.Equal("http://localhost:4317", options.OtlpEndpoint);
        Assert.Equal("Staging", options.Environment);
        Assert.False(options.EnableTraces);
        Assert.True(options.EnableMetrics);
        Assert.False(options.EnableLogs);
    }

    // -------------------------------------------------------------------------
    // OTLP endpoint URI validation — mirrors ConfigureTraceExporter logic
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("http://localhost:4317", true)]
    [InlineData("https://otel.example.com:4317", true)]
    [InlineData("https://my-collector.prod.example.com/v1/traces", true)]
    [InlineData("", false)]
    [InlineData("not-a-uri", false)]
    [InlineData("   ", false)]
    [InlineData("localhost:4317", true)]
    public void OtlpEndpoint_AbsoluteUriCheck_DeterminesExporterSelection(
        string endpoint, bool expectedIsAbsoluteUri)
    {
        // The same check used inside ConfigureTraceExporter / ConfigureMetricExporter
        // / ConfigureLogExporter to decide between OTLP and Console exporters.
        var isAbsoluteUri = Uri.TryCreate(endpoint, UriKind.Absolute, out _);

        Assert.Equal(expectedIsAbsoluteUri, isAbsoluteUri);
    }

    // -------------------------------------------------------------------------
    // KarveActivitySource — name constant and instance availability
    // -------------------------------------------------------------------------

    [Fact]
    public void KarveActivitySource_Name_IsExpectedString()
    {
        Assert.Equal("Karve.Invoicing.Api", KarveActivitySource.Name);
    }

    [Fact]
    public void KarveActivitySource_Instance_CanStartAndEndActivityWithoutThrowing()
    {
        // No listener registered → StartActivity returns null; that is expected
        // behavior and must not throw.
        var activity = KarveActivitySource.Instance.StartActivity("test.operation");
        activity?.AddTag("test.key", "test.value");
        activity?.Stop();
        // No assert needed — reaching here without exception is the contract.
    }

    // -------------------------------------------------------------------------
    // AddKarveOpenTelemetry registration — smoke tests for DI setup
    // -------------------------------------------------------------------------

    [Fact]
    public void AddKarveOpenTelemetry_DevelopmentEnvironment_DoesNotThrow()
    {
        var builder = WebApplication.CreateBuilder(
            new WebApplicationOptions { EnvironmentName = "Development" });

        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["OpenTelemetry:ServiceName"] = "test-service",
            ["DisableDataSeeding"] = "true",
        });

        var exception = Record.Exception(() => builder.AddKarveOpenTelemetry());

        Assert.Null(exception);
    }

    [Fact]
    public void AddKarveOpenTelemetry_ProductionWithValidOtlpEndpoint_DoesNotThrow()
    {
        var builder = WebApplication.CreateBuilder(
            new WebApplicationOptions { EnvironmentName = "Production" });

        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["OpenTelemetry:ServiceName"] = "test-service",
            ["OpenTelemetry:OtlpEndpoint"] = "http://localhost:4317",
            ["DisableDataSeeding"] = "true",
        });

        var exception = Record.Exception(() => builder.AddKarveOpenTelemetry());

        Assert.Null(exception);
    }

    [Fact]
    public void AddKarveOpenTelemetry_RegistersOpenTelemetryServices()
    {
        var builder = WebApplication.CreateBuilder(
            new WebApplicationOptions { EnvironmentName = "Development" });

        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["OpenTelemetry:ServiceName"] = "test-service",
            ["DisableDataSeeding"] = "true",
        });

        builder.AddKarveOpenTelemetry();

        var hasOtelDescriptor = builder.Services.Any(sd =>
            sd.ServiceType.FullName != null &&
            sd.ServiceType.FullName.Contains("OpenTelemetry", StringComparison.OrdinalIgnoreCase));

        Assert.True(hasOtelDescriptor, "OpenTelemetry services should be registered in the DI container.");
    }
}
