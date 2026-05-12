using System.Text.Json;
using PrintService.Commands;
using PrintService.Printing;
using PrintService.Protocol;
using Xunit;

namespace PrintService.IntegrationTests;

public class PrintIntegrationTests
{
    [Fact]
    public void FullPrintFlow_TextAndBarcode_ShouldGenerateValidZpl()
    {
        var generator = new ZplGenerator();
        var template = new PrintTemplate
        {
            Id = "integration-label",
            Name = "Integration Test Label",
            Version = "1.0",
            PageSize = new PageSize { Width = 100, Height = 60, Unit = "mm" },
            Elements =
            {
                new TemplateElement
                {
                    Type = "text",
                    X = 5,
                    Y = 5,
                    Font = "Arial",
                    FontSize = 12,
                    Bold = true,
                    Content = "Ship to: {{address}}"
                },
                new TemplateElement
                {
                    Type = "barcode",
                    X = 5,
                    Y = 30,
                    Code = "{{barcode}}",
                    Format = "Code128",
                    Height = 40,
                    ModuleWidth = 2,
                    ShowText = true
                },
                new TemplateElement
                {
                    Type = "qrcode",
                    X = 120,
                    Y = 5,
                    Content = "{{url}}",
                    Format = "QR",
                    Size = 8,
                    ErrorCorrection = "M"
                }
            }
        };

        var data = new Dictionary<string, object>
        {
            ["address"] = "123 Main St",
            ["barcode"] = "ABC-12345",
            ["url"] = "https://example.com/track/ABC-12345"
        };

        var result = generator.GenerateCommands(template, data, offsetX: 2, offsetY: 3);

        Assert.StartsWith("^XA", result);
        Assert.EndsWith("^XZ", result);
        Assert.Contains("^CI28", result);
        Assert.Contains("123 Main St", result);
        Assert.Contains("^BC", result);
        Assert.Contains("^FD>:ABC-12345", result);
        Assert.Contains("^BQ", result);
        Assert.Contains("^FDhttps://example.com/track/ABC-12345", result);
    }

    [Fact]
    public async Task PrintQueue_BatchProcessing_ShouldCompleteAllBatches()
    {
        var queue = new PrintQueue(batchSize: 3, batchIntervalMs: 1);
        var job = new PrintJob
        {
            RequestId = Guid.NewGuid(),
            TemplateId = "batch-test",
            Data = new Dictionary<string, object> { ["item"] = "widget" },
        Options = new PrintService.Printing.PrintOptions
        {
            Copies = 10,
            PrinterName = "TestPrinter"
        }
        };

        await queue.EnqueueAsync(job);
        Assert.Equal(PrintJobStatus.Pending, job.Status);

        var batchesProcessed = 0;

        await queue.ExecuteBatchAsync(
            job,
            (batchNumber, copies, _) =>
            {
                batchesProcessed++;
                return Task.CompletedTask;
            },
            CancellationToken.None);

        Assert.Equal(PrintJobStatus.Completed, job.Status);
        Assert.Equal(4, batchesProcessed);
        Assert.Equal(4, job.CurrentBatch);
        Assert.Equal(4, job.TotalBatches);
    }

    [Fact]
    public void TemplateEngine_RenderContent_ShouldReplaceAllVariables()
    {
        var content = "Order {{orderId}}: {{customer}} - {{product}} x {{quantity}}";
        var data = new Dictionary<string, object>
        {
            ["orderId"] = "ORD-2024-001",
            ["customer"] = "Acme Corp",
            ["product"] = "Widget Alpha",
            ["quantity"] = "42"
        };

        var result = TemplateEngine.RenderContent(content, data);

        Assert.Equal("Order ORD-2024-001: Acme Corp - Widget Alpha x 42", result);
        Assert.DoesNotContain("{{", result);
        Assert.DoesNotContain("}}", result);
    }

    [Fact]
    public async Task MessageHandler_PrintRequest_ShouldAcceptAndQueueJob()
    {
        var handler = new MessageHandler(
            new PrinterManager(),
            new PrintQueue(),
            _ => new PrintTemplate
            {
                Id = "test-template",
                Name = "Test",
                Version = "1.0"
            });

        var requestId = Guid.NewGuid();
        var request = new PrintRequest
        {
            Action = "print",
            RequestId = requestId,
            TemplateId = "test-template",
            Data = new Dictionary<string, object>
            {
                ["name"] = "Integration Test"
            },
            Options = new PrintService.Protocol.PrintOptions
            {
                Copies = 5,
                PrinterName = "Zebra ZT410"
            }
        };

        var json = JsonSerializer.Serialize(request);
        var result = await handler.HandleMessageAsync(json);

        var response = JsonSerializer.Deserialize<PrintResponse>(result);
        Assert.NotNull(response);
        Assert.Equal("accepted", response!.Status);
        Assert.Equal(requestId, response.RequestId);
        Assert.Equal("Print request accepted", response.Message);
    }
}
