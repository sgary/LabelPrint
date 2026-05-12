using System.Text.Json.Serialization;

namespace PrintService.Protocol;

public class PrintRequest
{
    [JsonPropertyName("action")]
    public string Action { get; set; } = "print";

    [JsonPropertyName("requestId")]
    public Guid RequestId { get; set; }

    [JsonPropertyName("templateId")]
    public string TemplateId { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public Dictionary<string, object>? Data { get; set; }

    [JsonPropertyName("options")]
    public PrintOptions? Options { get; set; }
}

public class PrintOptions
{
    [JsonPropertyName("copies")]
    public int Copies { get; set; } = 1;

    [JsonPropertyName("printerName")]
    public string? PrinterName { get; set; }

    [JsonPropertyName("offsetX")]
    public int OffsetX { get; set; } = 0;

    [JsonPropertyName("offsetY")]
    public int OffsetY { get; set; } = 0;
}
