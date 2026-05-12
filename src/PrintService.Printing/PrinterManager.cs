namespace PrintService.Printing;

public class PrinterManager
{
    private readonly OffsetManager _offsetManager;

    public PrinterManager()
        : this(new OffsetManager())
    {
    }

    public PrinterManager(OffsetManager offsetManager)
    {
        _offsetManager = offsetManager;
    }

    public Task PrintAsync(PrintBatch batch)
    {
        return Task.CompletedTask;
    }

    public async Task PrintAsync(PrintJob job, PrintTemplate template, int batchNumber, Action<string>? onZplGenerated = null)
    {
        var printerName = job.Options.PrinterName ?? "Default Printer";
        var generator = GetCommandGenerator(printerName);
        var storedOffset = _offsetManager.GetOffset(printerName, template.Id);

        var offsetX = storedOffset.X + job.Options.OffsetX;
        var offsetY = storedOffset.Y + job.Options.OffsetY;
        var generateMethod = generator.GetType().GetMethod("GenerateCommands");
        if (generateMethod is null)
        {
            throw new InvalidOperationException("Command generator does not expose GenerateCommands method.");
        }

        var commands = generateMethod.Invoke(generator, new object[] { template, job.Data, offsetX, offsetY })?.ToString() ?? string.Empty;

        if (onZplGenerated is not null)
        {
            onZplGenerated(commands);
        }

        await Task.CompletedTask;
    }

    public object GetCommandGenerator(string printerName)
    {
        var typeName = printerName.Contains("zebra", StringComparison.OrdinalIgnoreCase)
            ? "PrintService.Commands.ZplGenerator, PrintService.Commands"
            : "PrintService.Commands.BplGenerator, PrintService.Commands";

        var type = Type.GetType(typeName)
                   ?? throw new InvalidOperationException($"Unable to resolve command generator type: {typeName}");
        return Activator.CreateInstance(type)
               ?? throw new InvalidOperationException($"Unable to create command generator type: {typeName}");
    }

    public string GetPrinterStatus()
    {
        return "ready";
    }

    public IReadOnlyList<string> GetAvailablePrinters()
    {
        return new[] { "Default Printer" };
    }
}
