using System.Text.Json;
using System.Text.RegularExpressions;

namespace PrintService.Printing;

public static partial class TemplateEngine
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [GeneratedRegex("\\{\\{(\\w+)\\}\\}")]
    private static partial Regex VariableRegex();

    public static PrintTemplate ParseTemplate(string json)
    {
        return JsonSerializer.Deserialize<PrintTemplate>(json, JsonOptions)
               ?? throw new InvalidOperationException("Failed to parse template JSON.");
    }

    public static PrintTemplate LoadTemplate(string filePath)
    {
        var json = File.ReadAllText(filePath);
        return ParseTemplate(json);
    }

    public static string RenderContent(string content, IReadOnlyDictionary<string, object> data)
    {
        return VariableRegex().Replace(content, match =>
        {
            var key = match.Groups[1].Value;
            return data.TryGetValue(key, out var value) ? value?.ToString() ?? string.Empty : match.Value;
        });
    }

    public static PrintTemplate RenderElements(PrintTemplate template, IReadOnlyDictionary<string, object> data)
    {
        var rendered = new PrintTemplate
        {
            Id = template.Id,
            Name = template.Name,
            Version = template.Version,
            PageSize = new PageSize
            {
                Width = template.PageSize.Width,
                Height = template.PageSize.Height,
                Unit = template.PageSize.Unit
            },
            DefaultOffset = new Offset
            {
                X = template.DefaultOffset.X,
                Y = template.DefaultOffset.Y
            }
        };

        foreach (var element in template.Elements)
        {
            rendered.Elements.Add(new TemplateElement
            {
                Type = element.Type,
                X = element.X,
                Y = element.Y,
                Rotation = element.Rotation,
                Content = element.Content is null ? null : RenderContent(element.Content, data),
                Font = element.Font,
                FontSize = element.FontSize,
                Bold = element.Bold,
                Align = element.Align,
                Code = element.Code is null ? null : RenderContent(element.Code, data),
                Format = element.Format,
                Height = element.Height,
                ModuleWidth = element.ModuleWidth,
                ShowText = element.ShowText,
                Size = element.Size,
                ErrorCorrection = element.ErrorCorrection,
                Src = element.Src is null ? null : RenderContent(element.Src, data),
                X1 = element.X1,
                Y1 = element.Y1,
                X2 = element.X2,
                Y2 = element.Y2,
                LineWidth = element.LineWidth
            });
        }

        return rendered;
    }

    public static int MmToDots(double mm, int dpi = 203)
    {
        var inches = mm / 25.4;
        return (int)Math.Round(inches * dpi);
    }
}
