using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Karve.Invoicing.Application.BackgroundJobs.Jobs;
using Karve.Invoicing.Application.Interfaces;
using Karve.Invoicing.Domain.Enums;

namespace Karve.Invoicing.Application.BackgroundJobs.Handlers;

/// <summary>
/// Handler for <see cref="CheckOverdueInvoicesJob"/> background jobs.
/// This handler checks for overdue invoices and sends reminders to customers.
/// </summary>
/// <remarks>
/// Workflow:
/// 1. Retrieve all invoices for the company
/// 2. Filter for unpaid or partially paid invoices where due date has passed
/// 3. Update invoice status to "Overdue" if not already marked
/// 4. Log overdue invoices for auditing
/// 5. Enqueue email reminders for customers with overdue invoices (optional)
/// 6. Update the database with status changes
/// 7. Emit observability data (spans, logs, metrics)
/// 
/// This handler is designed to be scheduled periodically (e.g., daily) by the OverdueInvoiceScheduler.
/// </remarks>
public sealed class CheckOverdueInvoicesJobHandler : IBackgroundJobHandler<CheckOverdueInvoicesJob>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ILogger<CheckOverdueInvoicesJobHandler> _logger;

    public CheckOverdueInvoicesJobHandler(
        IInvoiceRepository invoiceRepository,
        ILogger<CheckOverdueInvoicesJobHandler> logger)
    {
        _invoiceRepository = invoiceRepository ?? throw new ArgumentNullException(nameof(invoiceRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Checks for and processes overdue invoices asynchronously.
    /// </summary>
    /// <param name="job">The CheckOverdueInvoicesJob containing the company ID.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task HandleAsync(CheckOverdueInvoicesJob job, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(job);

        var correlationId = Activity.Current?.GetBaggageItem("correlation.id")
            ?? Activity.Current?.GetTagItem("correlation.id")?.ToString()
            ?? Guid.NewGuid().ToString("N");

        using var activity = BackgroundJobActivitySource.Instance.StartActivity(nameof(CheckOverdueInvoicesJob), ActivityKind.Internal);
        activity?.SetTag("job.type", nameof(CheckOverdueInvoicesJob));
        activity?.SetTag("job.company_id", job.CompanyId);
        activity?.SetTag("correlation.id", correlationId);

        using var logScope = _logger.BeginScope(new Dictionary<string, object?>
        {
            ["JobType"] = nameof(CheckOverdueInvoicesJob),
            ["CompanyId"] = job.CompanyId,
            ["CorrelationId"] = correlationId
        });

        _logger.LogInformation(
            "Starting background job. JobType={JobType}, CompanyId={CompanyId}, CorrelationId={CorrelationId}",
            nameof(CheckOverdueInvoicesJob),
            job.CompanyId,
            correlationId);

        try
        {

            // Retrieve all invoices for this company
            var invoices = await _invoiceRepository.GetByCompanyIdAsync(job.CompanyId).ConfigureAwait(false);

            if (invoices is null || !invoices.Any())
            {
                _logger.LogInformation(
                    "No invoices found for company. JobType={JobType}, CompanyId={CompanyId}, CorrelationId={CorrelationId}",
                    nameof(CheckOverdueInvoicesJob),
                    job.CompanyId,
                    correlationId);
                activity?.SetStatus(ActivityStatusCode.Ok, "No invoices found.");
                return;
            }

            var today = DateTime.UtcNow.Date;
            var overdueInvoices = new List<Domain.Entities.Invoice>();
            var invoicesToUpdate = new List<Domain.Entities.Invoice>();

            // Filter for unpaid or partially paid invoices that are overdue
            foreach (var invoice in invoices)
            {
                var isPaid = invoice.Status == InvoiceStatus.Paid;
                var isAlreadyOverdue = invoice.Status == InvoiceStatus.Overdue;
                var isDueDatePassed = invoice.DueDate.Date < today;

                // If the invoice is not paid, the due date has passed, and it's not already marked overdue
                if (!isPaid && isDueDatePassed && !isAlreadyOverdue)
                {
                    overdueInvoices.Add(invoice);
                
                    // Update the invoice status to Overdue
                    invoice.Status = InvoiceStatus.Overdue;
                    invoicesToUpdate.Add(invoice);

                    var daysOverdue = (today - invoice.DueDate.Date).Days;

                    _logger.LogWarning(
                        "Overdue invoice detected. JobType={JobType}, CompanyId={CompanyId}, InvoiceId={InvoiceId}, InvoiceNumber={InvoiceNumber}, CustomerId={CustomerId}, CustomerName={CustomerName}, DueDate={DueDate}, DaysOverdue={DaysOverdue}, CorrelationId={CorrelationId}",
                        nameof(CheckOverdueInvoicesJob),
                        job.CompanyId,
                        invoice.Id,
                        invoice.InvoiceNumber,
                        invoice.CustomerId,
                        invoice.Customer?.Name ?? "Unknown",
                        invoice.DueDate.Date,
                        daysOverdue,
                        correlationId);
                }
                else if (isAlreadyOverdue && isPaid)
                {
                    // Edge case: Invoice is marked as Overdue but has now been paid
                    // Transition status to Paid
                    invoice.Status = InvoiceStatus.Paid;
                    invoicesToUpdate.Add(invoice);

                    _logger.LogInformation(
                        "Previously overdue invoice now paid. JobType={JobType}, CompanyId={CompanyId}, InvoiceId={InvoiceId}, InvoiceNumber={InvoiceNumber}, CorrelationId={CorrelationId}",
                        nameof(CheckOverdueInvoicesJob),
                        job.CompanyId,
                        invoice.Id,
                        invoice.InvoiceNumber,
                        correlationId);
                }
            }

            // Persist status changes to the database
            foreach (var invoice in invoicesToUpdate)
            {
                try
                {
                    await _invoiceRepository.UpdateAsync(invoice).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to update invoice status. JobType={JobType}, CompanyId={CompanyId}, InvoiceId={InvoiceId}, NewStatus={NewStatus}, CorrelationId={CorrelationId}",
                        nameof(CheckOverdueInvoicesJob),
                        job.CompanyId,
                        invoice.Id,
                        invoice.Status,
                        correlationId);
                }
            }

            _logger.LogInformation(
                "Background job completed. JobType={JobType}, CompanyId={CompanyId}, OverdueInvoicesFound={OverdueInvoicesFound}, InvoicesUpdated={InvoicesUpdated}, CorrelationId={CorrelationId}",
                nameof(CheckOverdueInvoicesJob),
                job.CompanyId,
                overdueInvoices.Count,
                invoicesToUpdate.Count,
                correlationId);

            activity?.SetTag("job.overdue_count", overdueInvoices.Count);
            activity?.SetTag("job.updated_count", invoicesToUpdate.Count);
            activity?.SetStatus(ActivityStatusCode.Ok);

            // TODO: Task Group D - Integrate with email reminder job enqueuing
            // Once the email service is available and SendInvoiceEmailJob is refined,
            // enqueue reminder emails for customers with overdue invoices:
            //
            // if (_backgroundJobQueue != null)
            // {
            //     foreach (var overdueInvoice in overdueInvoices)
            //     {
            //         var emailJob = new SendOverdueReminderEmailJob(
            //             overdueInvoice.Id,
            //             overdueInvoice.CompanyId,
            //             (today - overdueInvoice.DueDate.Date).Days  // days overdue
            //         );
            //         await _backgroundJobQueue.QueueAsync(emailJob);
            //     }
            // }
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogError(
                ex,
                "Background job failed. JobType={JobType}, CompanyId={CompanyId}, CorrelationId={CorrelationId}",
                nameof(CheckOverdueInvoicesJob),
                job.CompanyId,
                correlationId);
            throw;
        }
    }
}
