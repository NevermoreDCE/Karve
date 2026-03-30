namespace Karve.Invoicing.Application.BackgroundJobs;

/// <summary>
/// Abstraction for a background job queue.
/// </summary>
public interface IBackgroundJobQueue
{
    /// <summary>
    /// Enqueues a background job of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of job to enqueue.</typeparam>
    /// <param name="job">The job instance.</param>
    /// <returns>A value task that completes when the job is enqueued.</returns>
    ValueTask QueueAsync<T>(T job) where T : notnull;

    /// <summary>
    /// Dequeues the next background job of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of job to dequeue.</typeparam>
    /// <param name="cancellationToken">A cancellation token to observe while waiting.</param>
    /// <returns>A value task that returns the next enqueued job, or throws if cancelled.</returns>
    ValueTask<T> DequeueAsync<T>(CancellationToken cancellationToken) where T : notnull;
}
