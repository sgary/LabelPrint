using System.Text;
using PrintService.Barcodes;
using PrintService.Printing;

namespace PrintService.Commands;

public class ZplGenerator : ICommandGenerator
{
    public string GenerateCommands(PrintTemplate template, IReadOnlyDictionary<string, object> data, int offsetX, int offsetY)
    {
        var rendered = TemplateEngine.RenderElements(template, data);
        var sb = new StringBuilder();

        sb.Append("^XA");
        sb.Append("^CI28");

        var widthMm = rendered.PageSize.Width > 0 ? rendered.PageSize.Width : 50.8;
        var heightMm = rendered.PageSize.Height > 0 ? rendered.PageSize.Height : 25.4;
        sb.Append($"^PW{TemplateEngine.MmToDots(widthMm)}");
        sb.Append($"^LL{TemplateEngine.MmToDots(heightMm)}");

        foreach (var element in rendered.Elements)
        {
            switch (element.Type.ToLowerInvariant())
            {
                case "text":
                    sb.Append(BuildText(element, offsetX, offsetY));
                    break;
                case "barcode":
                    sb.Append(BuildBarcode(element, offsetX, offsetY));
                    break;
                case "qrcode":
                    sb.Append(BuildQrCode(element, offsetX, offsetY));
                    break;
                case "line":
                    sb.Append(BuildLine(element, offsetX, offsetY));
                    break;
            }
        }

        sb.Append("^XZ");
        return sb.ToString();
    }

    public string GetPrinterType() => "zebra";

    private static string BuildText(TemplateElement element, int offsetX, int offsetY)
    {
        var x = element.X + offsetX;
        var y = element.Y + offsetY;
        var font = MapFont(element.Font);
        var size = element.FontSize > 0 ? element.FontSize : 10;
        var height = size * 2;
        var width = element.Bold ? height : Math.Max(1, height / 2);
        return $"^FO{x},{y}^A{font},{height},{width}^FD{element.Content ?? string.Empty}^FS";
    }

    private static string BuildBarcode(TemplateElement element, int offsetX, int offsetY)
    {
        var x = element.X + offsetX;
        var y = element.Y + offsetY;
        var code = element.Code ?? string.Empty;
        var height = element.Height > 0 ? element.Height : 50;
        var moduleWidth = element.ModuleWidth > 0 ? element.ModuleWidth : 2;
        return BarcodeGenerator.GenerateZpl(code, ParseBarcodeFormat(element.Format), x, y, height, moduleWidth, element.ShowText, rotation: element.Rotation);
    }

    private static string BuildQrCode(TemplateElement element, int offsetX, int offsetY)
    {
        var x = element.X + offsetX;
        var y = element.Y + offsetY;
        var content = element.Content ?? string.Empty;
        var size = element.Size > 0 ? element.Size : 6;
        return QrCodeGenerator.GenerateZpl(content, ParseQrFormat(element.Format), x, y, size, ParseErrorCorrection(element.ErrorCorrection), element.Rotation);
    }

    private static string BuildLine(TemplateElement element, int offsetX, int offsetY)
    {
        var x = element.X1 + offsetX;
        var y = element.Y1 + offsetY;
        var width = Math.Max(0, element.X2 - element.X1);
        var height = Math.Max(0, element.Y2 - element.Y1);
        var lineWidth = element.LineWidth > 0 ? element.LineWidth : 1;
        return $"^FO{x},{y}^GB{width},{height},{lineWidth}^FS";
    }

    private static char MapFont(string? font) => font?.ToLowerInvariant() switch
    {
        "arial" => 'A',
        "times" => 'B',
        "courier" => 'C',
        _ => 'A'
    };

    private static BarcodeFormat ParseBarcodeFormat(string? format) => format?.ToLowerInvariant() switch
    {
        "code39" => BarcodeFormat.Code39,
        "ean13" => BarcodeFormat.EAN13,
        "ean8" => BarcodeFormat.EAN8,
        "upc_a" or "upca" => BarcodeFormat.UPC_A,
        "upc_e" or "upce" => BarcodeFormat.UPC_E,
        "itf" => BarcodeFormat.ITF,
        "codabar" => BarcodeFormat.Codabar,
        _ => BarcodeFormat.Code128
    };

    private static QrCodeFormat ParseQrFormat(string? format) => format?.ToLowerInvariant() switch
    {
        "datamatrix" => QrCodeFormat.DataMatrix,
        "pdf417" => QrCodeFormat.PDF417,
        "aztec" => QrCodeFormat.Aztec,
        _ => QrCodeFormat.QR
    };

    private static ErrorCorrectionLevel ParseErrorCorrection(string? ec) => ec?.ToUpperInvariant() switch
    {
        "L" => ErrorCorrectionLevel.L,
        "Q" => ErrorCorrectionLevel.Q,
        "H" => ErrorCorrectionLevel.H,
        _ => ErrorCorrectionLevel.M
    };
}
