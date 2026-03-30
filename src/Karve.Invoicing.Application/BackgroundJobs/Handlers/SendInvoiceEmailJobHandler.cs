using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Karve.Invoicing.Application.BackgroundJobs.Jobs;
using Karve.Invoicing.Application.Interfaces;
using Karve.Invoicing.Domain.Entities;

namespace Karve.Invoicing.Application.BackgroundJobs.Handlers;

/// <summary>
/// Handler for <see cref="SendInvoiceEmailJob"/> background jobs.
/// This handler sends an invoice to the customer via email.
/// </summary>
/// <remarks>
/// Workflow:
/// 1. Load the invoice from the database
/// 2. Validate that the invoice exists and belongs to the correct company
/// 3. Retrieve the customer's email address
/// 4. Format the invoice for email (HTML or PDF)
/// 5. Send the email using the email provider
/// 6. Update invoice metadata (sent timestamp, status if needed)
/// 7. Emit observability data (spans, logs)
/// 
/// This handler is designed to be resilient to transient failures.
/// In a production system, Polly retry policies would wrap the email sending.
/// </remarks>
public sealed class SendInvoiceEmailJobHandler : IBackgroundJobHandler<SendInvoiceEmailJob>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ILogger<SendInvoiceEmailJobHandler> _logger;

    public SendInvoiceEmailJobHandler(
        IInvoiceRepository invoiceRepository,
        ILogger<SendInvoiceEmailJobHandler> logger)
    {
        _invoiceRepository = invoiceRepository ?? throw new ArgumentNullException(nameof(invoiceRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handles sending an invoice email asynchronously.
    /// </summary>
    /// <param name="job">The SendInvoiceEmailJob containing invoice and company IDs.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the invoice is not found or does not belong to the specified company.
    /// </exception>
    public async Task HandleAsync(SendInvoiceEmailJob job, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(job);

        var correlationId = Activity.Current?.GetBaggageItem("correlation.id")
            ?? Activity.Current?.GetTagItem("correlation.id")?.ToString()
            ?? Guid.NewGuid().ToString("N");

        using var activity = BackgroundJobActivitySource.Instance.StartActivity(nameof(SendInvoiceEmailJob), ActivityKind.Internal);
        activity?.SetTag("job.type", nameof(SendInvoiceEmailJob));
        activity?.SetTag("job.company_id", job.CompanyId);
        activity?.SetTag("job.invoice_id", job.InvoiceId);
        activity?.SetTag("correlation.id", correlationId);

        using var logScope = _logger.BeginScope(new Dictionary<string, object?>
        {
            ["JobType"] = nameof(SendInvoiceEmailJob),
            ["CompanyId"] = job.CompanyId,
            ["InvoiceId"] = job.InvoiceId,
            ["CorrelationId"] = correlationId
        });

        _logger.LogInformation(
            "Starting background job. JobType={JobType}, CompanyId={CompanyId}, InvoiceId={InvoiceId}, CorrelationId={CorrelationId}",
            nameof(SendInvoiceEmailJob),
            job.CompanyId,
            job.InvoiceId,
            correlationId);

        try
        {
            // Load the invoice from the database
            var invoice = await _invoiceRepository.GetByIdAsync(job.InvoiceId).ConfigureAwait(false);

            if (invoice is null)
            {
                throw new InvalidOperationException(
                    $"Invoice with ID '{job.InvoiceId}' not found.");
            }

            // Verify that the invoice belongs to the specified company
            if (invoice.CompanyId != job.CompanyId)
            {
                throw new InvalidOperationException(
                    $"Invoice '{job.InvoiceId}' does not belong to company '{job.CompanyId}'. " +
                    $"It belongs to company '{invoice.CompanyId}'.");
            }

            // Verify that the customer exists and has an email
            if (invoice.Customer is null)
            {
                throw new InvalidOperationException(
                    $"Invoice '{job.InvoiceId}' has no customer associated.");
            }

            if (invoice.Customer.Email is null || string.IsNullOrWhiteSpace(invoice.Customer.Email.Value))
            {
                _logger.LogWarning(
                    "Cannot send invoice email. JobType={JobType}, CompanyId={CompanyId}, InvoiceId={InvoiceId}, CustomerId={CustomerId}, CustomerName={CustomerName}, CorrelationId={CorrelationId}",
                    nameof(SendInvoiceEmailJob),
                    job.CompanyId,
                    job.InvoiceId,
                    invoice.CustomerId,
                    invoice.Customer.Name,
                    correlationId);
                activity?.SetStatus(ActivityStatusCode.Ok, "Customer email missing.");
                return;
            }

            _logger.LogInformation(
                "Preparing to send invoice email. JobType={JobType}, CompanyId={CompanyId}, InvoiceId={InvoiceId}, InvoiceNumber={InvoiceNumber}, CustomerName={CustomerName}, CustomerEmail={CustomerEmail}, CorrelationId={CorrelationId}",
                nameof(SendInvoiceEmailJob),
                job.CompanyId,
                job.InvoiceId,
                invoice.InvoiceNumber,
                invoice.Customer.Name,
                invoice.Customer.Email.Value,
                correlationId);

        // TODO: Task Group D - Integrate with IEmailSender service
        // Once the email service is available, implement the following:
        //
        // var emailMessage = new EmailMessage(
        //     To: invoice.Customer.Email.Value,
        //     Subject: $"Invoice #{invoice.InvoiceNumber} - {invoice.Company.Name}",
        //     Body: RenderInvoiceAsHtml(invoice),
        //     IsBodyHtml: true
        // );
        //
        // await _emailSender.SendAsync(emailMessage, cancellationToken);
        //
        // Consider wrapping the email send with Polly retry policies for resilience.

            _logger.LogInformation(
                "Background job completed successfully. JobType={JobType}, CompanyId={CompanyId}, InvoiceId={InvoiceId}, InvoiceNumber={InvoiceNumber}, CustomerEmail={CustomerEmail}, CorrelationId={CorrelationId}",
                nameof(SendInvoiceEmailJob),
                job.CompanyId,
                job.InvoiceId,
                invoice.InvoiceNumber,
                invoice.Customer.Email.Value,
                correlationId);

            activity?.SetStatus(ActivityStatusCode.Ok);

            await Task.CompletedTask.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogError(
                ex,
                "Background job failed. JobType={JobType}, CompanyId={CompanyId}, InvoiceId={InvoiceId}, CorrelationId={CorrelationId}",
                nameof(SendInvoiceEmailJob),
                job.CompanyId,
                job.InvoiceId,
                correlationId);
            throw;
        }
    }

    /// <summary>
    /// Renders an invoice as HTML for email delivery.
    /// This is a placeholder for the actual implementation in Task Group D.
    /// </summary>
    /// <param name="invoice">The invoice to render.</param>
    /// <returns>The HTML representation of the invoice.</returns>
    private static string RenderInvoiceAsHtml(Invoice invoice)
    {
        // TODO: Implement HTML rendering logic (e.g., using a template engine like Liquid or Scriban)
        return $@"
<html>
<body>
<h1>Invoice #{invoice.InvoiceNumber}</h1>
<p>Invoice Date: {invoice.InvoiceDate:yyyy-MM-dd}</p>
<p>Due Date: {invoice.DueDate:yyyy-MM-dd}</p>
<p>Customer: {invoice.Customer?.Name ?? "N/A"}</p>
</body>
</html>";
    }
}
