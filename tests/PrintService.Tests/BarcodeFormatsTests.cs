using PrintService.Barcodes;
using Xunit;

namespace PrintService.Tests;

public class BarcodeFormatsTests
{
    [Fact]
    public void Enum_HasAllEightFormats()
    {
        Assert.Equal(8, Enum.GetValues<BarcodeFormat>().Length);
    }

    [Fact]
    public void Code128_ToZplCode_ReturnsBC()
    {
        Assert.Equal("BC", BarcodeFormat.Code128.ToZplCode());
    }

    [Fact]
    public void Code39_ToZplCode_ReturnsB3()
    {
        Assert.Equal("B3", BarcodeFormat.Code39.ToZplCode());
    }

    [Fact]
    public void Ean13_ToZplCode_ReturnsBE()
    {
        Assert.Equal("BE", BarcodeFormat.EAN13.ToZplCode());
    }

    [Fact]
    public void Ean8_ToZplCode_ReturnsB8()
    {
        Assert.Equal("B8", BarcodeFormat.EAN8.ToZplCode());
    }

    [Fact]
    public void UpcA_ToZplCode_ReturnsBU()
    {
        Assert.Equal("BU", BarcodeFormat.UPC_A.ToZplCode());
    }

    [Fact]
    public void UpcE_ToZplCode_ReturnsB9()
    {
        Assert.Equal("B9", BarcodeFormat.UPC_E.ToZplCode());
    }

    [Fact]
    public void Itf_ToZplCode_ReturnsBI()
    {
        Assert.Equal("BI", BarcodeFormat.ITF.ToZplCode());
    }

    [Fact]
    public void Codabar_ToZplCode_ReturnsBK()
    {
        Assert.Equal("BK", BarcodeFormat.Codabar.ToZplCode());
    }
}
