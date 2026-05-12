using System.Text.Json.Serialization;

namespace PrintService.Protocol;

public class PrinterStatusRequest
{
    [JsonPropertyName("action")]
    public string Action { get; set; } = "getPrinterStatus";
}

public class PrinterStatusResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("printerStatus")]
    public string PrinterStatus { get; set; } = string.Empty;
}

public class ErrorResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("requestId")]
    public Guid RequestId { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("errorCode")]
    public string ErrorCode { get; set; } = string.Empty;
}
