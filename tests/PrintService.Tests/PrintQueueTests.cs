using PrintService.Printing;
using Xunit;

namespace PrintService.Tests;

public class PrintQueueTests
{
    [Fact]
    public void CalculateBatchCount_ReturnsExpectedCeilingValue()
    {
        var queue = new PrintQueue(batchSize: 10, batchIntervalMs: 1);

        Assert.Equal(0, queue.CalculateBatchCount(0));
        Assert.Equal(1, queue.CalculateBatchCount(1));
        Assert.Equal(1, queue.CalculateBatchCount(10));
        Assert.Equal(2, queue.CalculateBatchCount(11));
        Assert.Equal(3, queue.CalculateBatchCount(25));
    }

    [Fact]
    public async Task EnqueueAsync_SetsTotalBatchesAndPendingStatus()
    {
        var queue = new PrintQueue(batchSize: 10, batchIntervalMs: 1);
        var job = new PrintJob
        {
            RequestId = Guid.NewGuid(),
            TemplateId = "template-1",
            Data = new Dictionary<string, object>
            {
                ["name"] = "Alice"
            },
            Options = new PrintOptions
            {
                Copies = 25,
                PrinterName = "Zebra GK420d"
            }
        };

        await queue.EnqueueAsync(job);

        Assert.Equal(PrintJobStatus.Pending, job.Status);
        Assert.Equal(3, job.TotalBatches);
        Assert.Equal(0, job.CurrentBatch);
    }

    [Fact]
    public void GetBatchCopies_ReturnsCorrectCopiesPerBatch()
    {
        var queue = new PrintQueue(batchSize: 10, batchIntervalMs: 1);
        var job = new PrintJob
        {
            Options = new PrintOptions
            {
                Copies = 25
            }
        };

        Assert.Equal(10, queue.GetBatchCopies(job, 1));
        Assert.Equal(10, queue.GetBatchCopies(job, 2));
        Assert.Equal(5, queue.GetBatchCopies(job, 3));
    }

    [Fact]
    public async Task DequeueAsync_ReturnsJobsInFifoOrder()
    {
        var queue = new PrintQueue(batchSize: 10, batchIntervalMs: 1);
        var first = new PrintJob { RequestId = Guid.NewGuid(), TemplateId = "A", Options = new PrintOptions { Copies = 1 } };
        var second = new PrintJob { RequestId = Guid.NewGuid(), TemplateId = "B", Options = new PrintOptions { Copies = 1 } };

        await queue.EnqueueAsync(first);
        await queue.EnqueueAsync(second);

        var dequeuedFirst = await queue.DequeueAsync();
        var dequeuedSecond = await queue.DequeueAsync();

        Assert.Same(first, dequeuedFirst);
        Assert.Same(second, dequeuedSecond);
    }

    [Fact]
    public async Task ExecuteBatchAsync_ProcessesAllBatchesAndRaisesEvents()
    {
        var queue = new PrintQueue(batchSize: 10, batchIntervalMs: 1);
        var job = new PrintJob
        {
            RequestId = Guid.NewGuid(),
            TemplateId = "template-1",
            Options = new PrintOptions { Copies = 25 }
        };
        await queue.EnqueueAsync(job);

        var started = 0;
        var completed = 0;
        var batchCompleted = 0;
        var progress = new List<(int Current, int Total)>();
        var printedBatches = new List<(int BatchNumber, int Copies)>();

        queue.JobStarted += _ => started++;
        queue.JobCompleted += _ => completed++;
        queue.BatchCompleted += (_, _, _) => batchCompleted++;
        queue.ProgressUpdated += (_, current, total) => progress.Add((current, total));

        await queue.ExecuteBatchAsync(
            job,
            (batchNumber, copies, _) =>
            {
                printedBatches.Add((batchNumber, copies));
                return Task.CompletedTask;
            },
            CancellationToken.None);

        Assert.Equal(PrintJobStatus.Completed, job.Status);
        Assert.Equal(3, job.CurrentBatch);
        Assert.Equal(1, started);
        Assert.Equal(1, completed);
        Assert.Equal(3, batchCompleted);
        Assert.Equal(new[] { (1, 3), (2, 3), (3, 3) }, progress);
        Assert.Equal(new[] { (1, 10), (2, 10), (3, 5) }, printedBatches);
    }

    [Fact]
    public async Task ExecuteBatchAsync_WhenCancelled_SetsCancelledStatus()
    {
        var queue = new PrintQueue(batchSize: 10, batchIntervalMs: 1);
        var job = new PrintJob
        {
            RequestId = Guid.NewGuid(),
            TemplateId = "template-1",
            Options = new PrintOptions { Copies = 20 }
        };
        await queue.EnqueueAsync(job);

        using var cts = new CancellationTokenSource();
        var callCount = 0;

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            queue.ExecuteBatchAsync(
                job,
                (_, _, ct) =>
                {
                    callCount++;
                    cts.Cancel();
                    ct.ThrowIfCancellationRequested();
                    return Task.CompletedTask;
                },
                cts.Token));

        Assert.Equal(PrintJobStatus.Cancelled, job.Status);
        Assert.True(callCount >= 1);
    }

    [Fact]
    public async Task EnqueueAndDequeueAsync_AreThreadSafeForConcurrentJobs()
    {
        var queue = new PrintQueue(batchSize: 10, batchIntervalMs: 1);
        var jobs = Enumerable.Range(1, 50)
            .Select(_ => new PrintJob
            {
                RequestId = Guid.NewGuid(),
                TemplateId = "template-concurrent",
                Options = new PrintOptions { Copies = 1 }
            })
            .ToList();

        await Task.WhenAll(jobs.Select(queue.EnqueueAsync));

        var dequeued = new List<PrintJob?>();
        for (var i = 0; i < jobs.Count; i++)
        {
            dequeued.Add(await queue.DequeueAsync());
        }

        Assert.Equal(50, dequeued.Count(j => j is not null));
        Assert.Equal(50, dequeued.Where(j => j is not null).Select(j => j!.JobId).Distinct().Count());
    }
}
