namespace PrintService.Barcodes;

public enum BarcodeFormat
{
    Code128,
    Code39,
    EAN13,
    EAN8,
    UPC_A,
    UPC_E,
    ITF,
    Codabar
}

public static class BarcodeFormatExtensions
{
    public static string ToZplCode(this BarcodeFormat format) => format switch
    {
        BarcodeFormat.Code128 => "BC",
        BarcodeFormat.Code39 => "B3",
        BarcodeFormat.EAN13 => "BE",
        BarcodeFormat.EAN8 => "B8",
        BarcodeFormat.UPC_A => "BU",
        BarcodeFormat.UPC_E => "B9",
        BarcodeFormat.ITF => "BI",
        BarcodeFormat.Codabar => "BK",
        _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
    };
}
