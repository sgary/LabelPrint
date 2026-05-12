using System.Text.Json;

namespace PrintService.Printing;

public class OffsetManager
{
    private readonly string? _storagePath;
    private readonly Dictionary<string, Offset> _offsets = new(StringComparer.OrdinalIgnoreCase);

    public int DefaultOffsetX { get; }

    public int DefaultOffsetY { get; }

    public OffsetManager(int defaultOffsetX = 0, int defaultOffsetY = 0)
    {
        DefaultOffsetX = defaultOffsetX;
        DefaultOffsetY = defaultOffsetY;
    }

    public OffsetManager(string storagePath)
    {
        _storagePath = storagePath;
        LoadFromStorage();
    }

    public void SetOffset(string printerName, string templateId, int x, int y)
    {
        _offsets[BuildKey(printerName, templateId)] = new Offset { X = x, Y = y };
        SaveToStorage();
    }

    public Offset GetOffset(string printerName, string templateId)
    {
        return _offsets.TryGetValue(BuildKey(printerName, templateId), out var offset)
            ? new Offset { X = offset.X, Y = offset.Y }
            : new Offset { X = DefaultOffsetX, Y = DefaultOffsetY };
    }

    public void Calibrate(string printerName, string templateId, int measuredX, int measuredY, int expectedX, int expectedY)
    {
        var deltaX = expectedX - measuredX;
        var deltaY = expectedY - measuredY;
        SetOffset(printerName, templateId, deltaX, deltaY);
    }

    private static string BuildKey(string printerName, string templateId) => $"{printerName}:{templateId}";

    private void LoadFromStorage()
    {
        if (string.IsNullOrWhiteSpace(_storagePath) || !File.Exists(_storagePath))
        {
            return;
        }

        var json = File.ReadAllText(_storagePath);
        var loaded = JsonSerializer.Deserialize<Dictionary<string, Offset>>(json);
        if (loaded is null)
        {
            return;
        }

        foreach (var kv in loaded)
        {
            _offsets[kv.Key] = kv.Value;
        }
    }

    private void SaveToStorage()
    {
        if (string.IsNullOrWhiteSpace(_storagePath))
        {
            return;
        }

        var dir = Path.GetDirectoryName(_storagePath);
        if (!string.IsNullOrWhiteSpace(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var json = JsonSerializer.Serialize(_offsets);
        File.WriteAllText(_storagePath, json);
    }
}
