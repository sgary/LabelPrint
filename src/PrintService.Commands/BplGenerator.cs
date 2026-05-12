using System.Text;
using PrintService.Printing;

namespace PrintService.Commands;

public class BplGenerator : ICommandGenerator
{
    public string GenerateCommands(PrintTemplate template, IReadOnlyDictionary<string, object> data, int offsetX, int offsetY)
    {
        var rendered = TemplateEngine.RenderElements(template, data);
        var sb = new StringBuilder();

        sb.Append("^@");
        sb.Append("^L");
        sb.Append($"^S{TemplateEngine.MmToDots(rendered.PageSize.Width)},{TemplateEngine.MmToDots(rendered.PageSize.Height)}");

        foreach (var element in rendered.Elements)
        {
            switch (element.Type.ToLowerInvariant())
            {
                case "text":
                    sb.Append($"^V{element.X + offsetX},{element.Y + offsetY}");
                    sb.Append($"^A{element.Content ?? string.Empty}");
                    break;
                case "barcode":
                    sb.Append($"^V{element.X + offsetX},{element.Y + offsetY}");
                    sb.Append($"^B{MapBarcodeType(element.Format)},{element.Height},{element.ModuleWidth},{element.Code ?? string.Empty}");
                    break;
                case "qrcode":
                    sb.Append($"^V{element.X + offsetX},{element.Y + offsetY}");
                    sb.Append($"^Q{element.Size},{element.ErrorCorrection ?? "M"},{element.Content ?? string.Empty}");
                    break;
            }
        }

        return sb.ToString();
    }

    public string GetPrinterType() => "brother-bpl";

    private static string MapBarcodeType(string? format) => format?.ToLowerInvariant() switch
    {
        "code128" => "C",
        "code39" => "3",
        "ean13" => "E",
        "ean8" => "8",
        "upc_a" or "upca" => "U",
        "upc_e" or "upce" => "9",
        "itf" => "I",
        "codabar" => "K",
        _ => "C"
    };
}
