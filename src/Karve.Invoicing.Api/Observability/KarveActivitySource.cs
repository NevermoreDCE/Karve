using System.Diagnostics;

namespace Karve.Invoicing.Api.Observability;

/// <summary>
/// Shared activity source names for custom backend tracing.
/// </summary>
public static class KarveActivitySource
{
    /// <summary>
    /// The activity source name used by custom API spans.
    /// </summary>
    public const string Name = "Karve.Invoicing.Api";

    /// <summary>
    /// Shared activity source instance.
    /// </summary>
    public static readonly ActivitySource Instance = new(Name);
}