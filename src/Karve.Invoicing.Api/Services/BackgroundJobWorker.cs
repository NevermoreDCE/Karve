using System.Diagnostics;
using Karve.Invoicing.Application.BackgroundJobs;
using Karve.Invoicing.Api.Observability;

namespace Karve.Invoicing.Api.Services;

/// <summary>
/// A hosted background worker service that processes queued background jobs asynchronously.
/// This service:
/// - Continuously monitors the background job queue
/// - Processes jobs by resolving and executing appropriate handlers
/// - Emits OpenTelemetry traces for observability
/// - Logs all job processing activity with structured logging
/// - Handles errors gracefully and continues processing
/// </summary>
public sealed class BackgroundJobWorker : BackgroundService
{
    private readonly IBackgroundJobQueue _jobQueue;
    private readonly ILogger<BackgroundJobWorker> _logger;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackgroundJobWorker"/> class.
    /// </summary>
    /// <param name="jobQueue">The background job queue to process jobs from.</param>
    /// <param name="logger">The logger for this service.</param>
    /// <param name="serviceProvider">The service provider for creating DI scopes.</param>
    /// <exception cref="ArgumentNullException">Thrown if any parameter is null.</exception>
    public BackgroundJobWorker(
        IBackgroundJobQueue jobQueue,
        ILogger<BackgroundJobWorker> logger,
        IServiceProvider serviceProvider)
    {
        _jobQueue = jobQueue ?? throw new ArgumentNullException(nameof(jobQueue));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <summary>
    /// Main execution loop for processing background jobs.
    /// This method runs continuously while the application is running,
    /// dequeuing and processing jobs from the background job queue.
    /// </summary>
    /// <param name="stoppingToken">A cancellation token that signals when the service should stop.</param>
    /// <returns>A task representing the background worker's execution.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background job worker started");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Dequeue the next background job
                    // Note: DequeueAsync<object> returns any job type; specific handler resolution
                    // happens in ExecuteJobAsync below (Task Group C will define handler patterns)
                    var job = await _jobQueue.DequeueAsync<object>(stoppingToken).ConfigureAwait(false);

                    // Execute the job with proper error handling and observability
                    await ExecuteJobAsync(job, stoppingToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // Expected when stoppingToken is cancelled
                    _logger.LogInformation("Background job worker cancellation requested");
                    break;
                }
                catch (Exception ex)
                {
                    // Log unexpected errors but continue processing
                    _logger.LogError(
                        ex,
                        "An unexpected error occurred while dequeuing a background job");
                }
            }
        }
        finally
        {
            _logger.LogInformation("Background job worker stopped");
        }
    }

    /// <summary>
    /// Executes a single background job with observability and error handling.
    /// 
    /// This method:
    /// 1. Creates a new OpenTelemetry span for the job
    /// 2. Creates a DI scope for the job's execution
    /// 3. Resolves the appropriate handler for the job type
    /// 4. Executes the handler with try/catch error handling
    /// 5. Sets the span status based on success/failure
    /// 6. Logs all activity with structured logging
    /// </summary>
    /// <param name="job">The background job to execute.</param>
    /// <param name="cancellationToken">A cancellation token to observe while executing.</param>
    /// <returns>A task representing the job's execution.</returns>
    private async Task ExecuteJobAsync(object job, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(job);

        var jobType = job.GetType();
        var jobTypeFullName = jobType.FullName ?? jobType.Name;
        var correlationId = Guid.NewGuid().ToString("N");
        var companyId = TryGetGuidProperty(job, "CompanyId");
        var invoiceId = TryGetGuidProperty(job, "InvoiceId");

        using var activity = KarveActivitySource.Instance.StartActivity(
            $"background_job.{jobType.Name}",
            ActivityKind.Internal);

        if (activity is not null)
        {
            activity.SetTag("job.type", jobTypeFullName);
            activity.SetTag("job.type_name", jobType.Name);
            activity.SetTag("correlation.id", correlationId);
            activity.AddBaggage("correlation.id", correlationId);

            if (companyId.HasValue)
            {
                activity.SetTag("job.company_id", companyId.Value);
            }

            if (invoiceId.HasValue)
            {
                activity.SetTag("job.invoice_id", invoiceId.Value);
            }
        }

        using var logScope = _logger.BeginScope(new Dictionary<string, object?>
        {
            ["CorrelationId"] = correlationId,
            ["JobType"] = jobType.Name,
            ["CompanyId"] = companyId,
            ["InvoiceId"] = invoiceId
        });

        _logger.LogInformation(
            "Processing background job. JobType={JobType}, JobTypeFullName={JobTypeFullName}, CorrelationId={CorrelationId}, CompanyId={CompanyId}, InvoiceId={InvoiceId}",
            jobType.Name,
            jobTypeFullName,
            correlationId,
            companyId,
            invoiceId);

        try
        {
            // Create a new DI scope for this job's execution
            using var scope = _serviceProvider.CreateScope();
            var scopedServiceProvider = scope.ServiceProvider;

            // Task Group C will define handler patterns (IBackgroundJobHandler<TJob>)
            // This is where we'll resolve handlers dynamically based on job type
            // For now, the structure is in place for handler resolution to be added

            await ExecuteJobWithHandlerAsync(job, scopedServiceProvider, jobType, cancellationToken).ConfigureAwait(false);

            activity?.SetStatus(ActivityStatusCode.Ok);
            _logger.LogInformation(
                "Background job processed successfully. JobType={JobType}, CorrelationId={CorrelationId}",
                jobType.Name,
                correlationId);
        }
        catch (OperationCanceledException)
        {
            activity?.SetStatus(ActivityStatusCode.Ok, "Job was cancelled");
            _logger.LogWarning(
                "Background job was cancelled. JobType={JobType}, CorrelationId={CorrelationId}",
                jobType.Name,
                correlationId);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddEvent(new ActivityEvent(
                "exception",
                tags: new ActivityTagsCollection
                {
                    ["exception.type"] = ex.GetType().FullName,
                    ["exception.message"] = ex.Message,
                    ["exception.stacktrace"] = ex.StackTrace
                }));

            _logger.LogError(
                ex,
                "Failed to process background job. JobType={JobType}, CorrelationId={CorrelationId}",
                jobType.Name,
                correlationId);
        }
    }

    /// <summary>
    /// Resolves and executes the appropriate handler for a background job.
    /// 
    /// This method uses reflection to dynamically locate and invoke the handler
    /// interface IBackgroundJobHandler&lt;TJob&gt; for the given job type.
    /// This pattern is defined in Task Group C.
    /// </summary>
    /// <param name="job">The background job to handle.</param>
    /// <param name="scopedServiceProvider">The scoped DI service provider.</param>
    /// <param name="jobType">The runtime type of the job.</param>
    /// <param name="cancellationToken">A cancellation token to observe while executing.</param>
    /// <returns>A task representing the handler's execution.</returns>
    /// <remarks>
    /// Handler resolution pattern (Task Group C):
    /// This method expects handlers to implement IBackgroundJobHandler&lt;TJob&gt;
    /// where TJob is the specific job type. The generic handler interface
    /// has a single method: Task HandleAsync(TJob job, CancellationToken cancellationToken);
    /// 
    /// Example handler registration (Program.cs):
    ///   builder.Services.AddScoped&lt;
    ///     IBackgroundJobHandler&lt;SendInvoiceEmailJob&gt;,
    ///     SendInvoiceEmailJobHandler&gt;();
    /// </remarks>
    private async Task ExecuteJobWithHandlerAsync(
        object job,
        IServiceProvider scopedServiceProvider,
        Type jobType,
        CancellationToken cancellationToken)
    {
        // Build the generic handler interface type: IBackgroundJobHandler<jobType>
        var handlerInterfaceType = typeof(IBackgroundJobHandler<>).MakeGenericType(jobType);

        // Attempt to resolve the handler from the DI container
        var handler = scopedServiceProvider.GetService(handlerInterfaceType);

        if (handler is null)
        {
            throw new InvalidOperationException(
                $"No handler registered for job type '{jobType.FullName}'. " +
                $"Register a handler in Program.cs using: " +
                $"builder.Services.AddScoped<IBackgroundJobHandler<{jobType.Name}>, YourHandlerClass>();");
        }

        // Invoke the handler's HandleAsync method using reflection
        // The generic handler has this signature: Task HandleAsync(TJob job, CancellationToken cancellationToken)
        var handleMethod = handlerInterfaceType.GetMethod("HandleAsync");

        if (handleMethod is null)
        {
            throw new InvalidOperationException(
                $"Handler for job type '{jobType.FullName}' does not have a HandleAsync method.");
        }

        var invokeResult = handleMethod.Invoke(handler, new[] { job, cancellationToken });

        if (invokeResult is Task task)
        {
            await task.ConfigureAwait(false);
        }
        else
        {
            throw new InvalidOperationException(
                $"Handler for job type '{jobType.FullName}' HandleAsync method did not return a Task.");
        }
    }

    private static Guid? TryGetGuidProperty(object job, string propertyName)
    {
        var property = job.GetType().GetProperty(propertyName);
        if (property?.PropertyType == typeof(Guid))
        {
            var value = property.GetValue(job);
            if (value is Guid guidValue)
            {
                return guidValue;
            }
        }

        return null;
    }
}
