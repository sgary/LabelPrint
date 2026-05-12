using PrintService.Barcodes;
using Xunit;

namespace PrintService.Tests;

public class QrCodeFormatsTests
{
    [Fact]
    public void QrCodeFormat_Enum_HasFourFormats()
    {
        Assert.Equal(4, Enum.GetValues<QrCodeFormat>().Length);
    }

    [Fact]
    public void QrCodeFormat_QR_ToZplCode_ReturnsBQ()
    {
        Assert.Equal("BQ", QrCodeFormat.QR.ToZplCode());
    }

    [Fact]
    public void QrCodeFormat_DataMatrix_ToZplCode_ReturnsBX()
    {
        Assert.Equal("BX", QrCodeFormat.DataMatrix.ToZplCode());
    }

    [Fact]
    public void QrCodeFormat_PDF417_ToZplCode_ReturnsB7()
    {
        Assert.Equal("B7", QrCodeFormat.PDF417.ToZplCode());
    }

    [Fact]
    public void QrCodeFormat_Aztec_ToZplCode_ReturnsBO()
    {
        Assert.Equal("BO", QrCodeFormat.Aztec.ToZplCode());
    }

    [Fact]
    public void ErrorCorrectionLevel_Enum_HasFourLevels()
    {
        Assert.Equal(4, Enum.GetValues<ErrorCorrectionLevel>().Length);
    }

    [Theory]
    [InlineData(ErrorCorrectionLevel.L, "L")]
    [InlineData(ErrorCorrectionLevel.M, "M")]
    [InlineData(ErrorCorrectionLevel.Q, "Q")]
    [InlineData(ErrorCorrectionLevel.H, "H")]
    public void ErrorCorrectionLevel_ToZplValue_ReturnsCorrectChar(ErrorCorrectionLevel level, string expected)
    {
        Assert.Equal(expected, level.ToZplValue());
    }
}
