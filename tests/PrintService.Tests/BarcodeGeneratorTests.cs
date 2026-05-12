using PrintService.Barcodes;
using Xunit;

namespace PrintService.Tests;

public class BarcodeGeneratorTests
{
    [Fact]
    public void GenerateZpl_Code128_ContainsCorrectCommand()
    {
        var result = BarcodeGenerator.GenerateZpl("ABC123", BarcodeFormat.Code128, 10, 20, 50);

        Assert.Contains("^FO10,20", result);
        Assert.Contains("^BC", result);
        Assert.Contains("^FD>:ABC123", result);
        Assert.Contains("^FS", result);
    }

    [Fact]
    public void GenerateZpl_Code39_ContainsCorrectCommand()
    {
        var result = BarcodeGenerator.GenerateZpl("TEST", BarcodeFormat.Code39, 30, 40, 60);

        Assert.Contains("^FO30,40", result);
        Assert.Contains("^B3", result);
        Assert.Contains("^FDTEST", result);
        Assert.Contains("^FS", result);
    }

    [Fact]
    public void GenerateZpl_EAN13_ContainsCorrectCommand()
    {
        var result = BarcodeGenerator.GenerateZpl("5901234123457", BarcodeFormat.EAN13, 0, 0, 50);

        Assert.Contains("^BE", result);
        Assert.Contains("^FD5901234123457", result);
    }

    [Fact]
    public void GenerateZpl_EAN8_ContainsCorrectCommand()
    {
        var result = BarcodeGenerator.GenerateZpl("96385074", BarcodeFormat.EAN8, 5, 5, 40);

        Assert.Contains("^B8", result);
        Assert.Contains("^FD96385074", result);
    }

    [Fact]
    public void GenerateZpl_UPC_A_ContainsCorrectCommand()
    {
        var result = BarcodeGenerator.GenerateZpl("012345678905", BarcodeFormat.UPC_A, 10, 10, 50);

        Assert.Contains("^BU", result);
        Assert.Contains("^FD012345678905", result);
    }

    [Fact]
    public void GenerateZpl_UPC_E_ContainsCorrectCommand()
    {
        var result = BarcodeGenerator.GenerateZpl("01234565", BarcodeFormat.UPC_E, 10, 10, 50);

        Assert.Contains("^B9", result);
        Assert.Contains("^FD01234565", result);
    }

    [Fact]
    public void GenerateZpl_ITF_ContainsCorrectCommand()
    {
        var result = BarcodeGenerator.GenerateZpl("1234567890", BarcodeFormat.ITF, 20, 30, 70);

        Assert.Contains("^BI", result);
        Assert.Contains("^FD1234567890", result);
    }

    [Fact]
    public void GenerateZpl_Codabar_ContainsCorrectCommand()
    {
        var result = BarcodeGenerator.GenerateZpl("A12345B", BarcodeFormat.Codabar, 15, 25, 60);

        Assert.Contains("^BK", result);
        Assert.Contains("^FDA12345B", result);
    }

    [Fact]
    public void GenerateZpl_EscapesCaretInData()
    {
        var result = BarcodeGenerator.GenerateZpl("ABC^123", BarcodeFormat.Code128, 0, 0, 50);

        Assert.Contains("^FD>:ABC^^123", result);
    }

    [Fact]
    public void GenerateZpl_EscapesTildeInData()
    {
        var result = BarcodeGenerator.GenerateZpl("ABC~123", BarcodeFormat.Code128, 0, 0, 50);

        Assert.Contains("^FD>:ABC~~123", result);
    }

    [Fact]
    public void GenerateZpl_WithModuleWidth_IncludesByParameter()
    {
        var result = BarcodeGenerator.GenerateZpl("TEST", BarcodeFormat.Code128, 0, 0, 50, moduleWidth: 3);

        Assert.Contains("^BC", result);
    }

    [Fact]
    public void GenerateZpl_WithShowTextFalse_IncludesNAfterHeight()
    {
        var result = BarcodeGenerator.GenerateZpl("TEST", BarcodeFormat.Code128, 0, 0, 50, showText: false);

        Assert.Contains("^BC", result);
    }

    [Fact]
    public void GenerateZpl_WithAboveTextPosition_IncludesY()
    {
        var result = BarcodeGenerator.GenerateZpl("TEST", BarcodeFormat.Code128, 0, 0, 50, textPosition: "above");

        Assert.Contains("^BC", result);
    }

    [Fact]
    public void GenerateZpl_WithRotation_IncludesOrientation()
    {
        var result = BarcodeGenerator.GenerateZpl("TEST", BarcodeFormat.Code128, 0, 0, 50, rotation: 90);

        Assert.Contains("^BC", result);
        Assert.Contains("^FD", result);
    }

    [Fact]
    public void GenerateBpl_ReturnsSimplifiedCommand()
    {
        var result = BarcodeGenerator.GenerateBpl("12345", BarcodeFormat.Code128, 10, 20, 50);

        Assert.Contains("Barcode", result);
        Assert.Contains("Code128", result);
        Assert.Contains("12345", result);
        Assert.Contains("10", result);
        Assert.Contains("20", result);
    }

    [Fact]
    public void GenerateZpl_AllFormats_ContainFO_FD_FS()
    {
        foreach (BarcodeFormat format in Enum.GetValues<BarcodeFormat>())
        {
            var result = BarcodeGenerator.GenerateZpl("DATA", format, 0, 0, 50);

            Assert.Contains("^FO0,0", result);
            Assert.Contains("^FS", result);
        }
    }

    [Fact]
    public void GenerateZpl_EscapesBothSpecialChars()
    {
        var result = BarcodeGenerator.GenerateZpl("A^B~C", BarcodeFormat.Code39, 0, 0, 50);

        Assert.Contains("^FDA^^B~~C", result);
    }
}
