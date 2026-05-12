using PrintService.Commands;
using PrintService.Printing;
using Xunit;

namespace PrintService.Tests;

public class ZplGeneratorTests
{
    [Fact]
    public void GenerateCommands_WrapsWithZplHeaderAndFooter()
    {
        var generator = new ZplGenerator();
        var template = new PrintTemplate
        {
            Id = "t1",
            Name = "label",
            Version = "1.0",
            PageSize = new PageSize { Width = 50.8, Height = 25.4, Unit = "mm" }
        };

        var result = generator.GenerateCommands(template, new Dictionary<string, object>(), 0, 0);

        Assert.StartsWith("^XA", result);
        Assert.Contains("^CI28", result);
        Assert.Contains("^PW406", result);
        Assert.Contains("^LL203", result);
        Assert.EndsWith("^XZ", result);
    }

    [Fact]
    public void GenerateCommands_TextElement_MapsFontAndUsesOffset()
    {
        var generator = new ZplGenerator();
        var template = new PrintTemplate
        {
            Id = "t2",
            Name = "text",
            Version = "1.0",
            Elements =
            {
                new TemplateElement
                {
                    Type = "text",
                    X = 10,
                    Y = 20,
                    Font = "Arial",
                    FontSize = 10,
                    Bold = true,
                    Content = "Hello {{name}}"
                }
            }
        };

        var result = generator.GenerateCommands(
            template,
            new Dictionary<string, object> { ["name"] = "World" },
            offsetX: 3,
            offsetY: 4);

        Assert.Contains("^FO13,24", result);
        Assert.Contains("^AA,20,20", result);
        Assert.Contains("^FDHello World", result);
    }

    [Fact]
    public void GenerateCommands_RoutesBarcodeQrAndLineElements()
    {
        var generator = new ZplGenerator();
        var template = new PrintTemplate
        {
            Id = "t3",
            Name = "mixed",
            Version = "1.0",
            Elements =
            {
                new TemplateElement
                {
                    Type = "barcode",
                    X = 5,
                    Y = 7,
                    Code = "{{code}}",
                    Format = "Code128",
                    Height = 50,
                    ModuleWidth = 2,
                    ShowText = true
                },
                new TemplateElement
                {
                    Type = "qrcode",
                    X = 40,
                    Y = 10,
                    Content = "{{url}}",
                    Format = "QR",
                    Size = 6,
                    ErrorCorrection = "M"
                },
                new TemplateElement
                {
                    Type = "line",
                    X1 = 10,
                    Y1 = 20,
                    X2 = 110,
                    Y2 = 20,
                    LineWidth = 2
                }
            }
        };

        var result = generator.GenerateCommands(
            template,
            new Dictionary<string, object>
            {
                ["code"] = "123456",
                ["url"] = "https://example.com"
            },
            offsetX: 1,
            offsetY: 2);

        Assert.Contains("^FO6,9", result);
        Assert.Contains("^BC", result);
        Assert.Contains("^FD>:123456", result);

        Assert.Contains("^FO41,12", result);
        Assert.Contains("^BQ", result);
        Assert.Contains("^FDhttps://example.com", result);

        Assert.Contains("^FO11,22^GB100,0,2^FS", result);
    }
}
