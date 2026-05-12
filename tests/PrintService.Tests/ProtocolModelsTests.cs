using System.Text.Json;
using PrintService.Protocol;
using Xunit;

namespace PrintService.Tests;

public class ProtocolModelsTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    [Fact]
    public void PrintRequest_DefaultAction_IsPrint()
    {
        var request = new PrintRequest();
        Assert.Equal("print", request.Action);
    }

    [Fact]
    public void PrintRequest_SerializesAndDeserializes()
    {
        var request = new PrintRequest
        {
            Action = "print",
            RequestId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
            TemplateId = "label-template-01",
            Data = new Dictionary<string, object>
            {
                ["name"] = "John Doe",
                ["orderId"] = "ORD-12345"
            },
            Options = new PrintOptions
            {
                Copies = 2,
                PrinterName = "Zebra-ZT410",
                OffsetX = 10,
                OffsetY = 20
            }
        };

        var json = JsonSerializer.Serialize(request, JsonOptions);
        var deserialized = JsonSerializer.Deserialize<PrintRequest>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("print", deserialized!.Action);
        Assert.Equal(Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"), deserialized.RequestId);
        Assert.Equal("label-template-01", deserialized.TemplateId);
        Assert.NotNull(deserialized.Data);
        Assert.Equal("John Doe", deserialized.Data["name"].ToString());
        Assert.Equal("ORD-12345", deserialized.Data["orderId"].ToString());
        Assert.NotNull(deserialized.Options);
        Assert.Equal(2, deserialized.Options.Copies);
        Assert.Equal("Zebra-ZT410", deserialized.Options.PrinterName);
        Assert.Equal(10, deserialized.Options.OffsetX);
        Assert.Equal(20, deserialized.Options.OffsetY);
    }

    [Fact]
    public void PrintRequest_SerializesToExpectedJson()
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
                Copies = 2,
                PrinterName = "Zebra-ZT410"
            }
        };

        var json = JsonSerializer.Serialize(request, JsonOptions);

        Assert.Contains("\"action\": \"print\"", json);
        Assert.Contains("\"requestId\": \"aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee\"", json);
        Assert.Contains("\"templateId\": \"label-template-01\"", json);
        Assert.Contains("\"copies\": 2", json);
        Assert.Contains("\"printerName\": \"Zebra-ZT410\"", json);
    }

    [Fact]
    public void PrintOptions_Defaults_AreCorrect()
    {
        var options = new PrintOptions();
        Assert.Equal(1, options.Copies);
        Assert.Null(options.PrinterName);
        Assert.Equal(0, options.OffsetX);
        Assert.Equal(0, options.OffsetY);
    }

    [Fact]
    public void PrintResponse_SerializesAndDeserializes()
    {
        var response = new PrintResponse
        {
            Status = "success",
            RequestId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
            Message = "Print job completed",
            PrintedCount = 5,
            Errors = new List<string>()
        };

        var json = JsonSerializer.Serialize(response, JsonOptions);
        var deserialized = JsonSerializer.Deserialize<PrintResponse>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("success", deserialized!.Status);
        Assert.Equal(Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"), deserialized.RequestId);
        Assert.Equal("Print job completed", deserialized.Message);
        Assert.Equal(5, deserialized.PrintedCount);
        Assert.NotNull(deserialized.Errors);
        Assert.Empty(deserialized.Errors);
    }

    [Fact]
    public void PrintResponse_WithErrors_SerializesAndDeserializes()
    {
        var response = new PrintResponse
        {
            Status = "failed",
            RequestId = Guid.Parse("bbbbbbbb-cccc-dddd-eeee-ffffffffffff"),
            Message = "Print job failed",
            PrintedCount = 0,
            Errors = new List<string> { "Printer not found", "Invalid template" }
        };

        var json = JsonSerializer.Serialize(response, JsonOptions);
        var deserialized = JsonSerializer.Deserialize<PrintResponse>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("failed", deserialized!.Status);
        Assert.Equal(2, deserialized.Errors.Count);
        Assert.Contains("Printer not found", deserialized.Errors);
        Assert.Contains("Invalid template", deserialized.Errors);
    }

    [Fact]
    public void ProgressResponse_ExtendsPrintResponse()
    {
        var progress = new ProgressResponse
        {
            Status = "progress",
            RequestId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
            Message = "Printing in progress",
            PrintedCount = 3,
            Current = 3,
            Total = 10
        };

        var json = JsonSerializer.Serialize(progress, JsonOptions);
        var deserialized = JsonSerializer.Deserialize<ProgressResponse>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("progress", deserialized!.Action);
        Assert.Equal("progress", deserialized.Status);
        Assert.Equal(3, deserialized.Current);
        Assert.Equal(10, deserialized.Total);
        Assert.Equal(3, deserialized.PrintedCount);
    }

    [Fact]
    public void PrinterStatusRequest_HasCorrectAction()
    {
        var request = new PrinterStatusRequest();
        Assert.Equal("getPrinterStatus", request.Action);
    }

    [Fact]
    public void PrinterStatusResponse_SerializesAndDeserializes()
    {
        var response = new PrinterStatusResponse
        {
            Status = "ok",
            PrinterStatus = "ready"
        };

        var json = JsonSerializer.Serialize(response, JsonOptions);
        var deserialized = JsonSerializer.Deserialize<PrinterStatusResponse>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("ok", deserialized!.Status);
        Assert.Equal("ready", deserialized.PrinterStatus);
    }

    [Fact]
    public void ErrorResponse_HasErrorCode()
    {
        var error = new ErrorResponse
        {
            Status = "error",
            RequestId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
            Message = "Something went wrong",
            ErrorCode = "PRINTER_NOT_FOUND"
        };

        var json = JsonSerializer.Serialize(error, JsonOptions);
        var deserialized = JsonSerializer.Deserialize<ErrorResponse>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("error", deserialized!.Status);
        Assert.Equal("PRINTER_NOT_FOUND", deserialized.ErrorCode);
        Assert.Equal("Something went wrong", deserialized.Message);
    }

    [Fact]
    public void ErrorResponse_SerializesErrorCodeInJson()
    {
        var error = new ErrorResponse
        {
            Status = "error",
            RequestId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
            Message = "Something went wrong",
            ErrorCode = "PRINTER_OFFLINE"
        };

        var json = JsonSerializer.Serialize(error, JsonOptions);

        Assert.Contains("\"errorCode\": \"PRINTER_OFFLINE\"", json);
    }
}
