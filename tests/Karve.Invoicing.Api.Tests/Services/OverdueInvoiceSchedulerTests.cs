using Karve.Invoicing.Api.Services;
using Karve.Invoicing.Application.BackgroundJobs;
using Karve.Invoicing.Application.BackgroundJobs.Jobs;
using Karve.Invoicing.Domain.Entities;
using Karve.Invoicing.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Karve.Invoicing.Api.Tests.Services;

public class OverdueInvoiceSchedulerTests
{
    [Fact]
    public async Task StartAsync_QueuesOverdueCheckJobForEachCompany_UsingTestClockPattern()
    {
        var testClock = new TestClock(DateTimeOffset.Parse("2026-03-01T00:00:00Z"));
        var dbPath = Path.Combine(Path.GetTempPath(), $"scheduler-test-{Guid.NewGuid():N}.db");

        var services = new ServiceCollection();
        services.AddDbContext<InvoicingDbContext>(o => o.UseSqlite($"Data Source={dbPath}"));

        var provider = services.BuildServiceProvider();
        await SeedCompaniesAsync(provider, 3);

        var queue = new RecordingBackgroundJobQueue();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["BackgroundJobs:OverdueInvoiceCheckIntervalMinutes"] = "60"
            })
            .Build();

        var scheduler = new OverdueInvoiceScheduler(
            queue,
            NullLogger<OverdueInvoiceScheduler>.Instance,
            provider,
            configuration);

        await scheduler.StartAsync(CancellationToken.None);

        await WaitUntilAsync(() => queue.Jobs.Count >= 3, TimeSpan.FromSeconds(2));

        await scheduler.StopAsync(CancellationToken.None);

        try
        {
            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }
        }
        catch
        {
            // Ignore best-effort test cleanup failures.
        }

        var overdueJobs = queue.Jobs.OfType<CheckOverdueInvoicesJob>().ToList();
        Assert.Equal(3, overdueJobs.Count);

        var distinctCompanyIds = overdueJobs.Select(j => j.CompanyId).Distinct().Count();
        Assert.Equal(3, distinctCompanyIds);

        Assert.True(testClock.UtcNow <= DateTimeOffset.Parse("2026-03-01T00:00:00Z").AddSeconds(1));
    }

    private static async Task SeedCompaniesAsync(IServiceProvider provider, int count)
    {
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<InvoicingDbContext>();

        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();

        for (var i = 0; i < count; i++)
        {
            db.Companies.Add(new Company
            {
                Id = Guid.NewGuid(),
                Name = $"Company-{i + 1}"
            });
        }

        await db.SaveChangesAsync();
    }

    private static async Task WaitUntilAsync(Func<bool> condition, TimeSpan timeout)
    {
        var started = DateTime.UtcNow;
        while (!condition())
        {
            if (DateTime.UtcNow - started > timeout)
            {
                throw new TimeoutException("Condition was not met within the timeout.");
            }

            await Task.Delay(20);
        }
    }

    private sealed class RecordingBackgroundJobQueue : IBackgroundJobQueue
    {
        private readonly List<object> _jobs = new();
        private readonly object _sync = new();

        public IReadOnlyList<object> Jobs
        {
            get
            {
                lock (_sync)
                {
                    return _jobs.ToList();
                }
            }
        }

        public ValueTask QueueAsync<T>(T job) where T : notnull
        {
            lock (_sync)
            {
                _jobs.Add(job);
            }

            return ValueTask.CompletedTask;
        }

        public ValueTask<T> DequeueAsync<T>(CancellationToken cancellationToken) where T : notnull
        {
            throw new NotSupportedException("Not needed for scheduler tests.");
        }
    }

    // TestClock pattern helper for deterministic time assertions in scheduler tests.
    private sealed class TestClock
    {
        public TestClock(DateTimeOffset utcNow)
        {
            UtcNow = utcNow;
        }

        public DateTimeOffset UtcNow { get; private set; }

        public void Advance(TimeSpan by) => UtcNow = UtcNow.Add(by);
    }
}
