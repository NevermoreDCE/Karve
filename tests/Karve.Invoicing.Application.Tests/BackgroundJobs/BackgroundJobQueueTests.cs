using Karve.Invoicing.Application.BackgroundJobs;

namespace Karve.Invoicing.Application.Tests.BackgroundJobs;

public class BackgroundJobQueueTests
{
    [Fact]
    public async Task QueueAndDequeue_ReturnsSameJobPayload()
    {
        var queue = new BackgroundJobQueue();
        var job = new TestJob(Guid.NewGuid(), "invoice-email");

        await queue.QueueAsync(job);
        var dequeued = await queue.DequeueAsync<TestJob>(CancellationToken.None);

        Assert.Equal(job.JobId, dequeued.JobId);
        Assert.Equal(job.Name, dequeued.Name);
    }

    [Fact]
    public async Task ConcurrentQueueing_DequeueAll_IsThreadSafe()
    {
        var queue = new BackgroundJobQueue();
        var totalJobs = 200;

        await Parallel.ForEachAsync(
            Enumerable.Range(0, totalJobs),
            async (i, ct) =>
            {
                await queue.QueueAsync(new TestJob(Guid.NewGuid(), $"job-{i}"));
            });

        var dequeuedCount = 0;
        var seenNames = new HashSet<string>(StringComparer.Ordinal);

        for (var i = 0; i < totalJobs; i++)
        {
            var job = await queue.DequeueAsync<TestJob>(CancellationToken.None);
            dequeuedCount++;
            seenNames.Add(job.Name);
        }

        Assert.Equal(totalJobs, dequeuedCount);
        Assert.Equal(totalJobs, seenNames.Count);
    }

    private sealed record TestJob(Guid JobId, string Name);
}
