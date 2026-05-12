namespace PrintService.Barcodes;

public static class QrCodeGenerator
{
    public static string GenerateZpl(
        string content,
        QrCodeFormat format,
        int x,
        int y,
        int size,
        ErrorCorrectionLevel errorCorrection = ErrorCorrectionLevel.M,
        int rotation = 0)
    {
        var escaped = EscapeData(content);
        var orientation = rotation switch
        {
            90 => "R",
            180 => "I",
            270 => "B",
            _ => "N"
        };

        var zplCode = format.ToZplCode();
        var ec = errorCorrection.ToZplValue();

        var command = format switch
        {
            QrCodeFormat.QR => $"^BQ{orientation},{size},{ec}",
            QrCodeFormat.DataMatrix => $"^BX{orientation},{size},0",
            QrCodeFormat.PDF417 => $"^B7{orientation},{size},{GetPdf417SecurityLevel(ec)},0",
            QrCodeFormat.Aztec => $"^BO{orientation},{size},{GetAztecErrorCorrection(ec)}",
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };

        return $"^FO{x},{y}{command}^FD{escaped}^FS";
    }

    private static string GetPdf417SecurityLevel(string ec)
    {
        return ec switch
        {
            "L" => "1",
            "M" => "3",
            "Q" => "5",
            "H" => "8",
            _ => "3"
        };
    }

    private static string GetAztecErrorCorrection(string ec)
    {
        return ec switch
        {
            "L" => "25",
            "M" => "50",
            "Q" => "75",
            "H" => "99",
            _ => "50"
        };
    }

    private static string EscapeData(string data)
    {
        return data
            .Replace("^", "^^")
            .Replace("~", "~~");
    }
}
