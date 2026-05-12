namespace PrintService.Barcodes;

public enum QrCodeFormat
{
    QR,
    DataMatrix,
    PDF417,
    Aztec
}

public enum ErrorCorrectionLevel
{
    L,
    M,
    Q,
    H
}

public static class QrCodeFormatExtensions
{
    public static string ToZplCode(this QrCodeFormat format) => format switch
    {
        QrCodeFormat.QR => "BQ",
        QrCodeFormat.DataMatrix => "BX",
        QrCodeFormat.PDF417 => "B7",
        QrCodeFormat.Aztec => "BO",
        _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
    };

    public static string ToZplValue(this ErrorCorrectionLevel level) => level switch
    {
        ErrorCorrectionLevel.L => "L",
        ErrorCorrectionLevel.M => "M",
        ErrorCorrectionLevel.Q => "Q",
        ErrorCorrectionLevel.H => "H",
        _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
    };
}
