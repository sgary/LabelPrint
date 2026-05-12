using System.Text.Json;
using PrintService.Printing;

namespace PrintService.Protocol;

public interface IMessageHandler
{
    Task<string> HandleMessageAsync(string message);
}

public class MessageHandler : IMessageHandler
{
    private readonly PrinterManager _printerManager;
    private readonly PrintQueue _printQueue;
    private readonly Func<string, PrintTemplate?> _templateLoader;
    private readonly Func<string, Task>? _sendProgressAsync;

    public MessageHandler(
        PrinterManager printerManager,
        PrintQueue printQueue,
        Func<string, PrintTemplate?> templateLoader,
        Func<string, Task>? sendProgressAsync = null)
    {
        _printerManager = printerManager;
        _printQueue = printQueue;
        _templateLoader = templateLoader;
        _sendProgressAsync = sendProgressAsync;
    }

    public async Task<string> HandleMessageAsync(string message)
    {
        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(message);
        }
        catch (JsonException)
        {
            var error = new ErrorResponse
            {
                Status = "error",
                ErrorCode = "INVALID_JSON",
                Message = "Failed to parse message as JSON"
            };
            return Task.FromResult(JsonSerializer.Serialize(error));
        }

        using (doc)
        {
            if (!doc.RootElement.TryGetProperty("action", out var actionProperty))
            {
                var error = new ErrorResponse
                {
                    Status = "error",
                    ErrorCode = "INVALID_JSON",
                    Message = "Message is missing 'action' field"
                };
                return Task.FromResult(JsonSerializer.Serialize(error));
            }

            var action = actionProperty.GetString() ?? string.Empty;

            return action switch
            {
                "print" => await HandlePrintAsync(doc.RootElement),
                "getPrinterStatus" => await HandlePrinterStatusAsync(),
                "getPrinters" => await HandleGetPrintersAsync(),
                _ => JsonSerializer.Serialize(new ErrorResponse
                {
                    Status = "error",
                    RequestId = TryGetRequestId(doc.RootElement),
                    ErrorCode = "UNKNOWN_ACTION",
                    Message = $"Unknown action: {action}"
                })
            };
        }
    }

    private async Task<string> HandlePrintAsync(JsonElement root)
    {
        PrintRequest? request;
        try
        {
            request = JsonSerializer.Deserialize<PrintRequest>(root.GetRawText());
        }
        catch (JsonException)
        {
            request = null;
        }

        if (request is null)
        {
            return JsonSerializer.Serialize(new ErrorResponse
            {
                Status = "error",
                RequestId = TryGetRequestId(root),
                ErrorCode = "INVALID_REQUEST",
                Message = "Invalid print request payload"
            });
        }

        var template = _templateLoader(request.TemplateId);
        if (template is null)
        {
            return JsonSerializer.Serialize(new ErrorResponse
            {
                Status = "error",
                RequestId = request.RequestId,
                ErrorCode = "TEMPLATE_NOT_FOUND",
                Message = $"Template not found: {request.TemplateId}"
            });
        }

        var job = new PrintJob
        {
            RequestId = request.RequestId,
            TemplateId = request.TemplateId,
            Data = request.Data ?? new Dictionary<string, object>(),
            Options = new PrintService.Printing.PrintOptions
            {
                Copies = request.Options?.Copies ?? 1,
                PrinterName = request.Options?.PrinterName,
                OffsetX = request.Options?.OffsetX ?? 0,
                OffsetY = request.Options?.OffsetY ?? 0
            }
        };

        await _printQueue.EnqueueAsync(job);

        _ = Task.Run(() => ProcessJobAsync(job));

        return JsonSerializer.Serialize(new PrintResponse
        {
            Status = "accepted",
            RequestId = request.RequestId,
            Message = "Print request accepted",
            PrintedCount = 0
        });
    }

    private async Task ProcessJobAsync(PrintJob job)
    {
        try
        {
            await _printQueue.ExecuteBatchAsync(
                job,
                async (batchNumber, copies, cancellationToken) =>
                {
                    var batch = new PrintBatch
                    {
                        Job = job,
                        Current = batchNumber,
                        Total = job.TotalBatches
                    };

                    await _printerManager.PrintAsync(batch);

                    if (_sendProgressAsync is not null)
                    {
                        var progress = new ProgressResponse
                        {
                            Status = "progress",
                            RequestId = job.RequestId,
                            Message = "Printing in progress",
                            PrintedCount = Math.Min(job.Options.Copies, batchNumber * copies),
                            Current = batchNumber,
                            Total = job.TotalBatches
                        };

                        await _sendProgressAsync(JsonSerializer.Serialize(progress));
                    }
                },
                CancellationToken.None);

            if (_sendProgressAsync is not null)
            {
                var done = new PrintResponse
                {
                    Status = "success",
                    RequestId = job.RequestId,
                    Message = "Print job completed",
                    PrintedCount = job.Options.Copies
                };

                await _sendProgressAsync(JsonSerializer.Serialize(done));
            }
        }
        catch (Exception ex)
        {
            if (_sendProgressAsync is null)
            {
                return;
            }

            var error = new ErrorResponse
            {
                Status = "error",
                RequestId = job.RequestId,
                ErrorCode = "PRINT_FAILED",
                Message = ex.Message
            };

            await _sendProgressAsync(JsonSerializer.Serialize(error));
        }
    }

    private Task<string> HandlePrinterStatusAsync()
    {
        var response = new PrinterStatusResponse
        {
            Status = "success",
            PrinterStatus = _printerManager.GetPrinterStatus()
        };

        return Task.FromResult(JsonSerializer.Serialize(response));
    }

    private Task<string> HandleGetPrintersAsync()
    {
        var response = new
        {
            status = "success",
            printers = _printerManager.GetAvailablePrinters()
        };

        return Task.FromResult(JsonSerializer.Serialize(response));
    }

    private static Guid TryGetRequestId(JsonElement root)
    {
        if (!root.TryGetProperty("requestId", out var requestIdProperty))
        {
            return Guid.Empty;
        }

        return Guid.TryParse(requestIdProperty.GetString(), out var requestId)
            ? requestId
            : Guid.Empty;
    }
}
