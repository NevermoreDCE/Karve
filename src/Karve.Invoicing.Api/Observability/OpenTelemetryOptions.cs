namespace Karve.Invoicing.Api.Observability;

/// <summary>
/// Vendor-neutral OpenTelemetry configuration for the API.
/// </summary>
public sealed class OpenTelemetryOptions
{
    /// <summary>
    /// OpenTelemetry configuration section name.
    /// </summary>
    public const string SectionName = "OpenTelemetry";

    /// <summary>
    /// Gets or sets the OTLP collector endpoint.
    /// </summary>
    public string OtlpEndpoint { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the logical service name reported by telemetry.
    /// </summary>
    public string ServiceName { get; set; } = "karve-invoicing-api";

    /// <summary>
    /// Gets or sets the deployment environment name.
    /// </summary>
    public string Environment { get; set; } = "Production";

    /// <summary>
    /// Gets or sets a value indicating whether traces are exported.
    /// </summary>
    public bool EnableTraces { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether metrics are exported.
    /// </summary>
    public bool EnableMetrics { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether logs are exported.
    /// </summary>
    public bool EnableLogs { get; set; } = true;
}
