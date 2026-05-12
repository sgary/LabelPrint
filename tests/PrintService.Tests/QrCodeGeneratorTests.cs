using PrintService.Barcodes;
using Xunit;

namespace PrintService.Tests;

public class QrCodeGeneratorTests
{
    [Fact]
    public void GenerateZpl_QR_ContainsCorrectCommand()
    {
        var result = QrCodeGenerator.GenerateZpl("https://example.com", QrCodeFormat.QR, 10, 20, 100);

        Assert.Contains("^FO10,20", result);
        Assert.Contains("^BQ", result);
        Assert.Contains("^FDhttps://example.com", result);
        Assert.Contains("^FS", result);
    }

    [Fact]
    public void GenerateZpl_DataMatrix_ContainsCorrectCommand()
    {
        var result = QrCodeGenerator.GenerateZpl("DATA", QrCodeFormat.DataMatrix, 5, 5, 50);

        Assert.Contains("^BX", result);
        Assert.Contains("^FDDATA", result);
    }

    [Fact]
    public void GenerateZpl_PDF417_ContainsCorrectCommand()
    {
        var result = QrCodeGenerator.GenerateZpl("PDF417 DATA", QrCodeFormat.PDF417, 0, 0, 80);

        Assert.Contains("^B7", result);
        Assert.Contains("^FDPDF417 DATA", result);
    }

    [Fact]
    public void GenerateZpl_Aztec_ContainsCorrectCommand()
    {
        var result = QrCodeGenerator.GenerateZpl("AZTEC DATA", QrCodeFormat.Aztec, 15, 25, 60);

        Assert.Contains("^BO", result);
        Assert.Contains("^FDAZTEC DATA", result);
    }

    [Fact]
    public void GenerateZpl_DefaultsToErrorCorrectionM()
    {
        var result = QrCodeGenerator.GenerateZpl("data", QrCodeFormat.QR, 0, 0, 50);

        Assert.Contains("^BQ", result);
    }

    [Fact]
    public void GenerateZpl_WithErrorCorrectionL_UsesL()
    {
        var result = QrCodeGenerator.GenerateZpl("data", QrCodeFormat.QR, 0, 0, 50, ErrorCorrectionLevel.L);

        Assert.Contains("^BQ", result);
    }

    [Fact]
    public void GenerateZpl_WithErrorCorrectionQ_UsesQ()
    {
        var result = QrCodeGenerator.GenerateZpl("data", QrCodeFormat.QR, 0, 0, 50, ErrorCorrectionLevel.Q);

        Assert.Contains("^BQ", result);
    }

    [Fact]
    public void GenerateZpl_WithErrorCorrectionH_UsesH()
    {
        var result = QrCodeGenerator.GenerateZpl("data", QrCodeFormat.QR, 0, 0, 50, ErrorCorrectionLevel.H);

        Assert.Contains("^BQ", result);
    }

    [Fact]
    public void GenerateZpl_EscapesCaretInData()
    {
        var result = QrCodeGenerator.GenerateZpl("data^value", QrCodeFormat.QR, 0, 0, 50);

        Assert.Contains("^FDdata^^value", result);
    }

    [Fact]
    public void GenerateZpl_EscapesTildeInData()
    {
        var result = QrCodeGenerator.GenerateZpl("data~value", QrCodeFormat.QR, 0, 0, 50);

        Assert.Contains("^FDdata~~value", result);
    }

    [Fact]
    public void GenerateZpl_WithRotation_IncludesFOWithCoordinates()
    {
        var result = QrCodeGenerator.GenerateZpl("data", QrCodeFormat.QR, 30, 40, 100, rotation: 90);

        Assert.Contains("^FO30,40", result);
    }

    [Fact]
    public void GenerateZpl_AllFormats_ContainFO_FD_FS()
    {
        foreach (QrCodeFormat format in Enum.GetValues<QrCodeFormat>())
        {
            var result = QrCodeGenerator.GenerateZpl("DATA", format, 0, 0, 50);

            Assert.Contains("^FO0,0", result);
            Assert.Contains("^FD", result);
            Assert.Contains("^FS", result);
        }
    }

    [Fact]
    public void GenerateZpl_EscapesBothSpecialChars()
    {
        var result = QrCodeGenerator.GenerateZpl("A^B~C", QrCodeFormat.QR, 0, 0, 50);

        Assert.Contains("^FDA^^B~~C", result);
    }
}
