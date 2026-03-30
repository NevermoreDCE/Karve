namespace Karve.Invoicing.Application.BackgroundJobs.Jobs;

/// <summary>
/// Background job to send an invoice via email to the associated customer.
/// This job is enqueued when an invoice is created or manually sent by the user.
/// </summary>
/// <param name="InvoiceId">The unique identifier of the invoice to send.</param>
/// <param name="CompanyId">The unique identifier of the company that owns the invoice.</param>
/// <remarks>
/// The handler will:
/// 1. Load the invoice and associated customer from the database
/// 2. Render the invoice as HTML or PDF (if supported)
/// 3. Send the email to the customer using the configured email provider
/// 4. Update the invoice status if necessary (e.g., from Draft to Sent)
/// </remarks>
public record SendInvoiceEmailJob(Guid InvoiceId, Guid CompanyId);
