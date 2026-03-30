using System.Threading.Channels;

namespace Karve.Invoicing.Application.BackgroundJobs;

/// <summary>
/// A thread-safe, high-performance background job queue implementation using System.Threading.Channels.
/// Supports generic job types and multiple concurrent producers/consumers.
/// </summary>
public sealed class BackgroundJobQueue : IBackgroundJobQueue
{
    private readonly Channel<object> _channel;

    public BackgroundJobQueue()
    {
        // Create an unbounded channel for background jobs
        var options = new UnboundedChannelOptions { SingleReader = false, SingleWriter = false };
        _channel = Channel.CreateUnbounded<object>(options);
    }

    /// <inheritdoc />
    public ValueTask QueueAsync<T>(T job) where T : notnull
    {
        ArgumentNullException.ThrowIfNull(job);
        return _channel.Writer.WriteAsync(job);
    }

    /// <inheritdoc />
    public async ValueTask<T> DequeueAsync<T>(CancellationToken cancellationToken) where T : notnull
    {
        while (await _channel.Reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
        {
            if (_channel.Reader.TryRead(out object? item) && item is T typedItem)
            {
                return typedItem;
            }
        }

        throw new OperationCanceledException("Background job queue was cancelled.");
    }
}
