using System.Text.Json;
using PrintService.Printing;
using PrintService.Protocol;
using Xunit;

namespace PrintService.Tests;

public class MessageHandlerTests
{
    private static MessageHandler CreateHandler(
        Func<string, PrintTemplate?>? templateLoader = null,
        Func<string, Task>? sendProgress = null)
    {
        return new MessageHandler(
            new PrinterManager(),
            new PrintQueue(),
            templateLoader ?? (_ => new PrintTemplate { Id = "label-template-01", Name = "Label", Version = "1" }),
            sendProgress);
    }

    [Fact]
    public async Task HandleMessageAsync_WithPrintRequest_ReturnsPrintResponse()
    {
        var handler = CreateHandler();
        var request = new PrintRequest
        {
            RequestId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
            TemplateId = "label-template-01",
            Data = new Dictionary<string, object>
            {
                ["name"] = "John Doe"
            },
            Options = new PrintService.Protocol.PrintOptions
            {
                Copies = 1
            }
        };
        var json = JsonSerializer.Serialize(request);

        var result = await handler.HandleMessageAsync(json);

        Assert.NotNull(result);
        var response = JsonSerializer.Deserialize<PrintResponse>(result);
        Assert.NotNull(response);
        Assert.Equal("accepted", response!.Status);
        Assert.Equal(request.RequestId, response.RequestId);
        Assert.Equal("Print request accepted", response.Message);
    }

    [Fact]
    public async Task HandleMessageAsync_WithMissingTemplate_ReturnsErrorResponse()
    {
        var handler = CreateHandler(_ => null);
        var request = new PrintRequest
        {
            RequestId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
            TemplateId = "missing-template"
        };

        var result = await handler.HandleMessageAsync(JsonSerializer.Serialize(request));

        var response = JsonSerializer.Deserialize<ErrorResponse>(result);
        Assert.NotNull(response);
        Assert.Equal("error", response!.Status);
        Assert.Equal("TEMPLATE_NOT_FOUND", response.ErrorCode);
    }

    [Fact]
    public async Task HandleMessageAsync_WithPrinterStatusRequest_ReturnsPrinterStatusResponse()
    {
        var handler = CreateHandler();
        var request = new PrinterStatusRequest();
        var json = JsonSerializer.Serialize(request);

        var result = await handler.HandleMessageAsync(json);

        Assert.NotNull(result);
        var response = JsonSerializer.Deserialize<PrinterStatusResponse>(result);
        Assert.NotNull(response);
        Assert.Equal("success", response!.Status);
        Assert.Equal("ready", response.PrinterStatus);
    }

    [Fact]
    public async Task HandleMessageAsync_WithGetPrintersRequest_ReturnsPrintersArray()
    {
        var handler = CreateHandler();
        var json = "{\"action\":\"getPrinters\"}";

        var result = await handler.HandleMessageAsync(json);
        using var doc = JsonDocument.Parse(result);

        Assert.Equal("success", doc.RootElement.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Array, doc.RootElement.GetProperty("printers").ValueKind);
    }

    [Fact]
    public async Task HandleMessageAsync_WithInvalidJson_ReturnsErrorResponse()
    {
        var handler = CreateHandler();
        var json = "this is not valid json";

        var result = await handler.HandleMessageAsync(json);

        Assert.NotNull(result);
        var response = JsonSerializer.Deserialize<ErrorResponse>(result);
        Assert.NotNull(response);
        Assert.Equal("error", response!.Status);
        Assert.Equal("INVALID_JSON", response.ErrorCode);
    }

    [Fact]
    public async Task HandleMessageAsync_WithUnknownAction_ReturnsErrorResponse()
    {
        var handler = CreateHandler();
        var json = "{\"action\":\"unknownAction\",\"requestId\":\"aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee\"}";

        var result = await handler.HandleMessageAsync(json);

        Assert.NotNull(result);
        var response = JsonSerializer.Deserialize<ErrorResponse>(result);
        Assert.NotNull(response);
        Assert.Equal("error", response!.Status);
        Assert.Equal("UNKNOWN_ACTION", response.ErrorCode);
    }

    [Fact]
    public async Task HandleMessageAsync_WithNullAction_ReturnsErrorResponse()
    {
        var handler = CreateHandler();
        var json = "{\"requestId\":\"aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee\"}";

        var result = await handler.HandleMessageAsync(json);

        Assert.NotNull(result);
        var response = JsonSerializer.Deserialize<ErrorResponse>(result);
        Assert.NotNull(response);
        Assert.Equal("error", response!.Status);
        Assert.Equal("INVALID_JSON", response.ErrorCode);
    }

    [Fact]
    public async Task HandleMessageAsync_PrintRequest_SendsProgressMessages()
    {
        var progressMessages = new List<string>();
        var handler = CreateHandler(sendProgress: message =>
        {
            lock (progressMessages)
            {
                progressMessages.Add(message);
            }

            return Task.CompletedTask;
        });

        var request = new PrintRequest
        {
            RequestId = Guid.NewGuid(),
            TemplateId = "label-template-01",
            Options = new PrintService.Protocol.PrintOptions { Copies = 2 }
        };

        var result = await handler.HandleMessageAsync(JsonSerializer.Serialize(request));
        var accepted = JsonSerializer.Deserialize<PrintResponse>(result);
        Assert.Equal("accepted", accepted!.Status);

        await Task.Delay(150);

        lock (progressMessages)
        {
            Assert.NotEmpty(progressMessages);
        }
    }
}
