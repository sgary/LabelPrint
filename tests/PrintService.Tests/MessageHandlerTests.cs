using System.Text.Json;
using PrintService.Protocol;
using Xunit;

namespace PrintService.Tests;

public class MessageHandlerTests
{
    private readonly MessageHandler _handler = new();

    [Fact]
    public async Task HandleMessageAsync_WithPrintRequest_ReturnsPrintResponse()
    {
        var request = new PrintRequest
        {
            RequestId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
            TemplateId = "label-template-01",
            Data = new Dictionary<string, object>
            {
                ["name"] = "John Doe"
            },
            Options = new PrintOptions
            {
                Copies = 1
            }
        };
        var json = JsonSerializer.Serialize(request);

        var result = await _handler.HandleMessageAsync(json);

        Assert.NotNull(result);
        var response = JsonSerializer.Deserialize<PrintResponse>(result);
        Assert.NotNull(response);
        Assert.Equal("success", response!.Status);
        Assert.Equal(request.RequestId, response.RequestId);
    }

    [Fact]
    public async Task HandleMessageAsync_WithPrinterStatusRequest_ReturnsPrinterStatusResponse()
    {
        var request = new PrinterStatusRequest();
        var json = JsonSerializer.Serialize(request);

        var result = await _handler.HandleMessageAsync(json);

        Assert.NotNull(result);
        var response = JsonSerializer.Deserialize<PrinterStatusResponse>(result);
        Assert.NotNull(response);
        Assert.Equal("ok", response!.Status);
        Assert.Equal("ready", response.PrinterStatus);
    }

    [Fact]
    public async Task HandleMessageAsync_WithInvalidJson_ReturnsErrorResponse()
    {
        var json = "this is not valid json";

        var result = await _handler.HandleMessageAsync(json);

        Assert.NotNull(result);
        var response = JsonSerializer.Deserialize<ErrorResponse>(result);
        Assert.NotNull(response);
        Assert.Equal("error", response!.Status);
        Assert.Equal("INVALID_JSON", response.ErrorCode);
    }

    [Fact]
    public async Task HandleMessageAsync_WithUnknownAction_ReturnsErrorResponse()
    {
        var json = "{\"action\":\"unknownAction\",\"requestId\":\"aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee\"}";

        var result = await _handler.HandleMessageAsync(json);

        Assert.NotNull(result);
        var response = JsonSerializer.Deserialize<ErrorResponse>(result);
        Assert.NotNull(response);
        Assert.Equal("error", response!.Status);
        Assert.Equal("UNKNOWN_ACTION", response.ErrorCode);
    }

    [Fact]
    public async Task HandleMessageAsync_WithNullAction_ReturnsErrorResponse()
    {
        var json = "{\"requestId\":\"aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee\"}";

        var result = await _handler.HandleMessageAsync(json);

        Assert.NotNull(result);
        var response = JsonSerializer.Deserialize<ErrorResponse>(result);
        Assert.NotNull(response);
        Assert.Equal("error", response!.Status);
        Assert.Equal("INVALID_JSON", response.ErrorCode);
    }
}
