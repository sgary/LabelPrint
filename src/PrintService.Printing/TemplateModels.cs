using System.Text.Json.Serialization;

namespace PrintService.Printing;

public class PrintTemplate
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("pageSize")]
    public PageSize PageSize { get; set; } = new();

    [JsonPropertyName("elements")]
    public List<TemplateElement> Elements { get; set; } = new();

    [JsonPropertyName("defaultOffset")]
    public Offset DefaultOffset { get; set; } = new();
}

public class PageSize
{
    [JsonPropertyName("width")]
    public double Width { get; set; }

    [JsonPropertyName("height")]
    public double Height { get; set; }

    [JsonPropertyName("unit")]
    public string Unit { get; set; } = "mm";
}

public class Offset
{
    [JsonPropertyName("x")]
    public int X { get; set; }

    [JsonPropertyName("y")]
    public int Y { get; set; }
}

public class TemplateElement
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("x")]
    public int X { get; set; }

    [JsonPropertyName("y")]
    public int Y { get; set; }

    [JsonPropertyName("rotation")]
    public int Rotation { get; set; }

    // text
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("font")]
    public string? Font { get; set; }

    [JsonPropertyName("fontSize")]
    public int FontSize { get; set; }

    [JsonPropertyName("bold")]
    public bool Bold { get; set; }

    [JsonPropertyName("align")]
    public string? Align { get; set; }

    // barcode
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("format")]
    public string? Format { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }

    [JsonPropertyName("moduleWidth")]
    public int ModuleWidth { get; set; }

    [JsonPropertyName("showText")]
    public bool ShowText { get; set; }

    // qrcode
    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("errorCorrection")]
    public string? ErrorCorrection { get; set; }

    // image
    [JsonPropertyName("src")]
    public string? Src { get; set; }

    // line
    [JsonPropertyName("x1")]
    public int X1 { get; set; }

    [JsonPropertyName("y1")]
    public int Y1 { get; set; }

    [JsonPropertyName("x2")]
    public int X2 { get; set; }

    [JsonPropertyName("y2")]
    public int Y2 { get; set; }

    [JsonPropertyName("lineWidth")]
    public int LineWidth { get; set; }
}
