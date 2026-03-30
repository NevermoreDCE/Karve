namespace Karve.Invoicing.Application.BackgroundJobs.Jobs;

/// <summary>
/// Background job to check for overdue invoices and send reminders to customers.
/// This job is enqueued by the OverdueInvoiceScheduler on a periodic basis (e.g., daily).
/// </summary>
/// <param name="CompanyId">The unique identifier of the company whose invoices should be checked.</param>
/// <remarks>
/// The handler will:
/// 1. Query all unpaid invoices for the company where the due date has passed
/// 2. For each overdue invoice, determine if a reminder email should be sent
/// 3. Send reminder emails to customers with customizable reminders (e.g., "Your invoice is X days overdue")
/// 4. Update invoice status to "Overdue" if not already marked
/// 5. Log all activity for audit and observability
/// </remarks>
public record CheckOverdueInvoicesJob(Guid CompanyId);
