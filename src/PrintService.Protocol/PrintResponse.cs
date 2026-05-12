using System.Text.Json.Serialization;

namespace PrintService.Protocol;

public class PrintResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("requestId")]
    public Guid RequestId { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("printedCount")]
    public int PrintedCount { get; set; }

    [JsonPropertyName("errors")]
    public List<string> Errors { get; set; } = new();
}

public class ProgressResponse : PrintResponse
{
    [JsonPropertyName("action")]
    public string Action { get; set; } = "progress";

    [JsonPropertyName("current")]
    public int Current { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }
}
