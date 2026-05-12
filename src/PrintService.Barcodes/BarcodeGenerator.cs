namespace PrintService.Barcodes;

public static class BarcodeGenerator
{
    public static string GenerateZpl(
        string data,
        BarcodeFormat format,
        int x,
        int y,
        int height,
        int moduleWidth = 2,
        bool showText = true,
        string textPosition = "below",
        int rotation = 0)
    {
        var escaped = EscapeData(data);
        var orientation = rotation switch
        {
            90 => "R",
            180 => "I",
            270 => "B",
            _ => "N"
        };

        var prefix = format == BarcodeFormat.Code128 ? ">:" : "";
        var zplCode = format.ToZplCode();
        var showTextVal = showText ? "Y" : "N";
        var textAbove = textPosition == "above" ? "Y" : "N";

        return $"^FO{x},{y}^BY{moduleWidth}^{zplCode}{orientation},{height},{showTextVal},{textAbove}^FD{prefix}{escaped}^FS";
    }

    public static string GenerateBpl(
        string data,
        BarcodeFormat format,
        int x,
        int y,
        int height,
        int moduleWidth = 2)
    {
        return $"Barcode {format}: {data} at ({x},{y}) height={height} width={moduleWidth}";
    }

    private static string EscapeData(string data)
    {
        return data
            .Replace("^", "^^")
            .Replace("~", "~~");
    }
}
