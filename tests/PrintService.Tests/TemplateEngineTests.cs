using PrintService.Printing;
using Xunit;

namespace PrintService.Tests;

public class TemplateEngineTests
{
    [Fact]
    public void ParseTemplate_DeserializesTemplateAndElements()
    {
        const string json = """
        {
          "id": "shipping-label",
          "name": "Shipping Label",
          "version": "1.0",
          "pageSize": { "width": 100, "height": 50, "unit": "mm" },
          "defaultOffset": { "x": 2, "y": 3 },
          "elements": [
            {
              "type": "text",
              "x": 10,
              "y": 15,
              "content": "Order {{orderId}}",
              "font": "Arial",
              "fontSize": 10,
              "bold": true,
              "align": "left"
            }
          ]
        }
        """;

        var template = TemplateEngine.ParseTemplate(json);

        Assert.Equal("shipping-label", template.Id);
        Assert.Equal("Shipping Label", template.Name);
        Assert.Equal("1.0", template.Version);
        Assert.Equal(100, template.PageSize.Width);
        Assert.Equal(50, template.PageSize.Height);
        Assert.Equal("mm", template.PageSize.Unit);
        Assert.Equal(2, template.DefaultOffset.X);
        Assert.Equal(3, template.DefaultOffset.Y);
        Assert.Single(template.Elements);
        Assert.Equal("text", template.Elements[0].Type);
        Assert.Equal("Order {{orderId}}", template.Elements[0].Content);
    }

    [Fact]
    public void RenderContent_ReplacesKnownVariablesAndKeepsUnknown()
    {
        var content = "Order {{orderId}} for {{customer}} / {{missing}}";
        var data = new Dictionary<string, object>
        {
            ["orderId"] = "A1001",
            ["customer"] = "Alice"
        };

        var result = TemplateEngine.RenderContent(content, data);

        Assert.Equal("Order A1001 for Alice / {{missing}}", result);
    }

    [Fact]
    public void RenderElements_RendersVariablesInSupportedFields()
    {
        var template = new PrintTemplate
        {
            Id = "t1",
            Name = "template",
            Version = "1",
            Elements =
            {
                new TemplateElement
                {
                    Type = "text",
                    Content = "Name: {{name}}"
                },
                new TemplateElement
                {
                    Type = "barcode",
                    Code = "{{barcode}}"
                },
                new TemplateElement
                {
                    Type = "qrcode",
                    Content = "https://example.com/{{id}}"
                },
                new TemplateElement
                {
                    Type = "image",
                    Src = "{{imagePath}}"
                }
            }
        };

        var data = new Dictionary<string, object>
        {
            ["name"] = "Bob",
            ["barcode"] = "1234567890",
            ["id"] = "42",
            ["imagePath"] = "C:/images/logo.png"
        };

        var rendered = TemplateEngine.RenderElements(template, data);

        Assert.Equal("Name: Bob", rendered.Elements[0].Content);
        Assert.Equal("1234567890", rendered.Elements[1].Code);
        Assert.Equal("https://example.com/42", rendered.Elements[2].Content);
        Assert.Equal("C:/images/logo.png", rendered.Elements[3].Src);
    }

    [Fact]
    public void MmToDots_ConvertsUsingDefault203Dpi()
    {
        var dots = TemplateEngine.MmToDots(25.4);

        Assert.Equal(203, dots);
    }
}
