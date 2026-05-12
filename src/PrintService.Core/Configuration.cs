using System.Text.Json;

namespace PrintService.Core;

public class Configuration
{
    public ServerConfig Server { get; set; } = new();
    public LoggingConfig Logging { get; set; } = new();
    public PrintingConfig Printing { get; set; } = new();
    public TemplatesConfig Templates { get; set; } = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static Configuration Load(string path)
    {
        if (!File.Exists(path))
            return new Configuration();

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<Configuration>(json)
               ?? throw new InvalidOperationException(
                   "Failed to deserialize configuration: deserialization returned null.");
    }

    public void Save(string path)
    {
        var json = JsonSerializer.Serialize(this, JsonOptions);
        var dir = System.IO.Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        File.WriteAllText(path, json);
    }
}

public class ServerConfig
{
    public int Port { get; set; } = 8080;
    public string BindAddress { get; set; } = "127.0.0.1";
}

public class LoggingConfig
{
    public string Level { get; set; } = "info";
    public string Path { get; set; } = @"C:\ProgramData\PrintService\logs";
}

public class PrintingConfig
{
    public int BatchSize { get; set; } = 10;
    public int BatchIntervalMs { get; set; } = 200;
    public int DefaultOffsetX { get; set; } = 0;
    public int DefaultOffsetY { get; set; } = 0;
}

public class TemplatesConfig
{
    public string Path { get; set; } = @"C:\ProgramData\PrintService\templates";
}
