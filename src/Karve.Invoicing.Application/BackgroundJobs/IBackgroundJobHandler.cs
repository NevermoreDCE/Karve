namespace Karve.Invoicing.Application.BackgroundJobs;

/// <summary>
/// Marker interface for background job handlers.
/// Implement this interface for each background job type you want to handle.
/// </summary>
/// <typeparam name="TJob">The type of background job this handler processes.</typeparam>
/// <remarks>
/// Handlers are registered in the DI container as scoped services, which means
/// a new instance will be created for each job processing operation. This allows
/// handlers to maintain isolated transaction contexts and perform database operations safely.
/// 
/// Example implementation:
/// <code>
/// public class SendInvoiceEmailJobHandler : IBackgroundJobHandler&lt;SendInvoiceEmailJob&gt;
/// {
///     private readonly IInvoiceRepository _invoiceRepository;
///     private readonly IEmailSender _emailSender;
/// 
///     public SendInvoiceEmailJobHandler(
///         IInvoiceRepository invoiceRepository,
///         IEmailSender emailSender)
///     {
///         _invoiceRepository = invoiceRepository;
///         _emailSender = emailSender;
///     }
/// 
///     public async Task HandleAsync(SendInvoiceEmailJob job, CancellationToken cancellationToken)
///     {
///         var invoice = await _invoiceRepository.GetByIdAsync(job.InvoiceId, cancellationToken);
///         if (invoice != null)
///         {
///             var message = new EmailMessage(...)
///             await _emailSender.SendAsync(message, cancellationToken);
///         }
///     }
/// }
/// </code>
/// 
/// Registering handlers in Program.cs:
/// <code>
/// builder.Services.AddScoped&lt;
///     IBackgroundJobHandler&lt;SendInvoiceEmailJob&gt;,
///     SendInvoiceEmailJobHandler&gt;();
/// </code>
/// </remarks>
public interface IBackgroundJobHandler<in TJob>
{
    /// <summary>
    /// Handles the specified background job asynchronously.
    /// </summary>
    /// <param name="job">The background job to handle.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task representing the asynchronous handling operation.</returns>
    Task HandleAsync(TJob job, CancellationToken cancellationToken);
}
