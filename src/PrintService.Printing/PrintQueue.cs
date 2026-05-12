using System.Collections.Concurrent;

namespace PrintService.Printing;

public class PrintBatch
{
    public PrintJob Job { get; set; } = new();

    public int Current { get; set; }

    public int Total { get; set; }
}

public class PrintQueue
{
    private readonly ConcurrentQueue<PrintJob> _queue = new();
    private readonly int _batchSize;
    private readonly int _batchIntervalMs;

    public event Action<PrintJob>? JobStarted;
    public event Action<PrintJob>? JobCompleted;
    public event Action<PrintJob, int, int>? BatchCompleted;
    public event Action<PrintJob, int, int>? ProgressUpdated;

    public PrintQueue(int batchSize = 10, int batchIntervalMs = 200)
    {
        _batchSize = Math.Max(1, batchSize);
        _batchIntervalMs = Math.Max(0, batchIntervalMs);
    }

    public Task EnqueueAsync(PrintJob job)
    {
        job.TotalBatches = CalculateBatchCount(job.Options.Copies);
        job.CurrentBatch = 0;
        job.Status = PrintJobStatus.Pending;
        _queue.Enqueue(job);
        return Task.CompletedTask;
    }

    public Task<PrintJob?> DequeueAsync()
    {
        return Task.FromResult(_queue.TryDequeue(out var job) ? job : null);
    }

    public void Clear()
    {
        while (_queue.TryDequeue(out _))
        {
        }
    }

    public async Task ExecuteBatchAsync(
        PrintJob job,
        Func<int, int, CancellationToken, Task> printBatchFunc,
        CancellationToken cancellationToken)
    {
        job.Status = PrintJobStatus.Processing;
        JobStarted?.Invoke(job);

        try
        {
            for (var batchNumber = 1; batchNumber <= job.TotalBatches; batchNumber++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                job.Status = PrintJobStatus.Printing;
                job.CurrentBatch = batchNumber;

                var batchCopies = GetBatchCopies(job, batchNumber);
                await printBatchFunc(batchNumber, batchCopies, cancellationToken);

                BatchCompleted?.Invoke(job, batchNumber, batchCopies);
                ProgressUpdated?.Invoke(job, batchNumber, job.TotalBatches);

                if (batchNumber < job.TotalBatches && _batchIntervalMs > 0)
                {
                    await Task.Delay(_batchIntervalMs, cancellationToken);
                }
            }

            job.Status = PrintJobStatus.Completed;
            JobCompleted?.Invoke(job);
        }
        catch (OperationCanceledException)
        {
            job.Status = PrintJobStatus.Cancelled;
            throw;
        }
        catch (Exception ex)
        {
            job.Status = PrintJobStatus.Failed;
            job.ErrorMessage = ex.Message;
            throw;
        }
    }

    public int CalculateBatchCount(int copies)
    {
        if (copies <= 0)
        {
            return 0;
        }

        return (int)Math.Ceiling(copies / (double)_batchSize);
    }

    public int GetBatchCopies(PrintJob job, int batchNumber)
    {
        var totalBatches = job.TotalBatches > 0
            ? job.TotalBatches
            : CalculateBatchCount(job.Options.Copies);

        if (batchNumber < 1 || batchNumber > totalBatches)
        {
            throw new ArgumentOutOfRangeException(nameof(batchNumber));
        }

        var consumed = (batchNumber - 1) * _batchSize;
        return Math.Min(_batchSize, Math.Max(0, job.Options.Copies - consumed));
    }
}
