namespace Karve.Invoicing.Api.Observability;

/// <summary>
/// Configuration options for Serilog structured logging.
/// </summary>
public sealed class SerilogOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Serilog";

    /// <summary>
    /// Gets or sets the default minimum log level.
    /// </summary>
    public string MinimumLevel { get; set; } = "Information";

    /// <summary>
    /// Gets or sets the Microsoft namespace minimum log level.
    /// </summary>
    public string MicrosoftMinimumLevel { get; set; } = "Warning";

    /// <summary>
    /// Gets or sets a value indicating whether JSON console output is enabled.
    /// </summary>
    public bool EnableJsonConsole { get; set; } = true;
}