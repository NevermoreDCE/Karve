using Karve.Invoicing.Api.Services;
using Karve.Invoicing.Application.BackgroundJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Karve.Invoicing.Api.Tests.Services;

public class BackgroundJobWorkerIntegrationTests
{
    [Fact]
    public async Task EnqueuedJob_IsProcessedByWorker_HandlerSideEffectObserved()
    {
        var completion = new TaskCompletionSource<Guid>(TaskCreationOptions.RunContinuationsAsynchronously);

        var services = new ServiceCollection();
        services.AddScoped<IBackgroundJobHandler<TestWorkerJob>>(_ => new TestWorkerJobHandler(completion));

        var provider = services.BuildServiceProvider();
        var queue = new BackgroundJobQueue();
        var worker = new BackgroundJobWorker(queue, NullLogger<BackgroundJobWorker>.Instance, provider);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await worker.StartAsync(cts.Token);

        var companyId = Guid.NewGuid();
        await queue.QueueAsync(new TestWorkerJob(companyId));

        var completed = await Task.WhenAny(completion.Task, Task.Delay(TimeSpan.FromSeconds(3), cts.Token));
        Assert.Same(completion.Task, completed);
        var handledCompanyId = await completion.Task;
        Assert.Equal(companyId, handledCompanyId);

        await worker.StopAsync(CancellationToken.None);
    }

    private sealed record TestWorkerJob(Guid CompanyId);

    private sealed class TestWorkerJobHandler : IBackgroundJobHandler<TestWorkerJob>
    {
        private readonly TaskCompletionSource<Guid> _completion;

        public TestWorkerJobHandler(TaskCompletionSource<Guid> completion)
        {
            _completion = completion;
        }

        public Task HandleAsync(TestWorkerJob job, CancellationToken cancellationToken)
        {
            _completion.TrySetResult(job.CompanyId);
            return Task.CompletedTask;
        }
    }
}
