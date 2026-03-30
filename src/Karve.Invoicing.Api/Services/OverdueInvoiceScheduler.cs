using System.Diagnostics;
using Karve.Invoicing.Api.Observability;
using Karve.Invoicing.Application.BackgroundJobs;
using Karve.Invoicing.Application.BackgroundJobs.Jobs;
using Karve.Invoicing.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Karve.Invoicing.Api.Services;

/// <summary>
/// Periodically enqueues overdue-invoice background jobs for each company.
/// </summary>
public sealed class OverdueInvoiceScheduler : BackgroundService
{
    private const int DefaultIntervalMinutes = 15;

    private readonly IBackgroundJobQueue _backgroundJobQueue;
    private readonly ILogger<OverdueInvoiceScheduler> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _interval;

    /// <summary>
    /// Initializes a new instance of the <see cref="OverdueInvoiceScheduler"/> class.
    /// </summary>
    public OverdueInvoiceScheduler(
        IBackgroundJobQueue backgroundJobQueue,
        ILogger<OverdueInvoiceScheduler> logger,
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _backgroundJobQueue = backgroundJobQueue ?? throw new ArgumentNullException(nameof(backgroundJobQueue));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        var configuredMinutes = configuration.GetValue<int?>("BackgroundJobs:OverdueInvoiceCheckIntervalMinutes");
        var intervalMinutes = configuredMinutes is > 0 ? configuredMinutes.Value : DefaultIntervalMinutes;
        _interval = TimeSpan.FromMinutes(intervalMinutes);
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Overdue invoice scheduler started with interval {IntervalMinutes} minute(s).",
            _interval.TotalMinutes);

        // Trigger one run at startup, then continue on interval.
        await EnqueueOverdueChecksAsync(stoppingToken).ConfigureAwait(false);

        using var timer = new PeriodicTimer(_interval);
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
            {
                await EnqueueOverdueChecksAsync(stoppingToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Overdue invoice scheduler cancellation requested.");
        }
        finally
        {
            _logger.LogInformation("Overdue invoice scheduler stopped.");
        }
    }

    private async Task EnqueueOverdueChecksAsync(CancellationToken cancellationToken)
    {
        using var activity = KarveActivitySource.Instance.StartActivity("background.overdue_invoice_scheduler", ActivityKind.Internal);
        activity?.SetTag("job.name", "overdue_invoice_scheduler");
        activity?.SetTag("job.trigger", "timer");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<InvoicingDbContext>();

            // Scheduler runs outside user HTTP context, so we intentionally bypass tenant filters.
            var companyIds = await dbContext.Companies
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Select(c => c.Id)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            var queuedCount = 0;
            foreach (var companyId in companyIds)
            {
                await _backgroundJobQueue
                    .QueueAsync(new CheckOverdueInvoicesJob(companyId))
                    .ConfigureAwait(false);
                queuedCount++;
            }

            activity?.SetTag("company.count", companyIds.Count);
            activity?.SetTag("jobs.enqueued", queuedCount);
            activity?.SetStatus(ActivityStatusCode.Ok);

            _logger.LogInformation(
                "Overdue scheduler cycle completed. Companies={CompanyCount}, JobsEnqueued={JobsEnqueued}",
                companyIds.Count,
                queuedCount);
        }
        catch (OperationCanceledException)
        {
            activity?.SetStatus(ActivityStatusCode.Ok, "Scheduler cycle canceled.");
            throw;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogError(ex, "Overdue scheduler cycle failed.");
        }
    }
}
