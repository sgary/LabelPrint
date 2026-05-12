namespace PrintService.Printing;

public enum PrintJobStatus
{
    Pending,
    Processing,
    Printing,
    Completed,
    Failed,
    Cancelled
}

public class PrintOptions
{
    public int Copies { get; set; } = 1;

    public string? PrinterName { get; set; }

    public int OffsetX { get; set; }

    public int OffsetY { get; set; }
}

public class PrintJob
{
    public Guid JobId { get; set; } = Guid.NewGuid();

    public Guid RequestId { get; set; }

    public string TemplateId { get; set; } = string.Empty;

    public Dictionary<string, object> Data { get; set; } = new();

    public PrintOptions Options { get; set; } = new();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public PrintJobStatus Status { get; set; } = PrintJobStatus.Pending;

    public string? ErrorMessage { get; set; }

    public int CurrentBatch { get; set; }

    public int TotalBatches { get; set; }
}
