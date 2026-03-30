using System.Diagnostics;

namespace Karve.Invoicing.Application.BackgroundJobs;

/// <summary>
/// Shared activity source for background job spans emitted from the application layer.
/// </summary>
public static class BackgroundJobActivitySource
{
    /// <summary>
    /// The activity source name used by background jobs.
    /// </summary>
    public const string Name = "Karve.Invoicing.BackgroundJobs";

    /// <summary>
    /// Shared activity source instance.
    /// </summary>
    public static readonly ActivitySource Instance = new(Name);
}