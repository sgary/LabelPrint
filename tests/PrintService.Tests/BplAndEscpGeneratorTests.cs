using PrintService.Commands;
using PrintService.Printing;
using Xunit;

namespace PrintService.Tests;

public class BplAndEscpGeneratorTests
{
    [Fact]
    public void BplGenerator_GenerateCommands_ContainsBrotherControlCommands()
    {
        var generator = new BplGenerator();
        var template = new PrintTemplate
        {
            Id = "bpl-1",
            Name = "Brother",
            Version = "1.0",
            PageSize = new PageSize { Width = 50.8, Height = 25.4, Unit = "mm" },
            Elements =
            {
                new TemplateElement
                {
                    Type = "barcode",
                    X = 10,
                    Y = 20,
                    Format = "Code128",
                    Code = "{{code}}",
                    Height = 60,
                    ModuleWidth = 2
                },
                new TemplateElement
                {
                    Type = "qrcode",
                    X = 30,
                    Y = 40,
                    Content = "{{url}}",
                    Size = 6,
                    ErrorCorrection = "M"
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
            1,
            2);

        Assert.Contains("^@", result);
        Assert.Contains("^L", result);
        Assert.Contains("^S406,203", result);
        Assert.Contains("^V11,22", result);
        Assert.Contains("^BC,60,2,123456", result);
        Assert.Contains("^V31,42", result);
        Assert.Contains("^Q6,M,https://example.com", result);
    }

    [Fact]
    public void EscpGenerator_GenerateCommands_ReturnsPlaceholderMessage()
    {
        var generator = new EscpGenerator();

        var result = generator.GenerateCommands(new PrintTemplate(), new Dictionary<string, object>(), 0, 0);

        Assert.Contains("not implemented", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Windows GDI", result, StringComparison.OrdinalIgnoreCase);
    }
}
