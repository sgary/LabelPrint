using System.Text.Json;
using PrintService.Core;
using Xunit;

namespace PrintService.Tests;

public class ConfigurationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    [Fact]
    public void Load_WhenFileNotExists_CreatesDefaultConfig()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var configPath = Path.Combine(tempDir, "config.json");

        try
        {
            var config = Configuration.Load(configPath);

            Assert.NotNull(config);
            Assert.NotNull(config.Server);
            Assert.NotNull(config.Logging);
            Assert.NotNull(config.Printing);
            Assert.NotNull(config.Templates);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var config = new Configuration();

        // Server defaults
        Assert.Equal(8080, config.Server.Port);
        Assert.Equal("127.0.0.1", config.Server.BindAddress);

        // Logging defaults
        Assert.Equal("info", config.Logging.Level);
        Assert.Equal(@"C:\ProgramData\PrintService\logs", config.Logging.Path);

        // Printing defaults
        Assert.Equal(10, config.Printing.BatchSize);
        Assert.Equal(200, config.Printing.BatchIntervalMs);
        Assert.Equal(0, config.Printing.DefaultOffsetX);
        Assert.Equal(0, config.Printing.DefaultOffsetY);

        // Templates defaults
        Assert.Equal(@"C:\ProgramData\PrintService\templates", config.Templates.Path);
    }

    [Fact]
    public void Load_WhenFileExists_ReadsConfig()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var configPath = Path.Combine(tempDir, "config.json");

        try
        {
            // Arrange: write a known config
            var expected = new Configuration
            {
                Server = new ServerConfig
                {
                    Port = 9000,
                    BindAddress = "0.0.0.0"
                },
                Logging = new LoggingConfig
                {
                    Level = "debug",
                    Path = "/var/log/printservice"
                },
                Printing = new PrintingConfig
                {
                    BatchSize = 5,
                    BatchIntervalMs = 500,
                    DefaultOffsetX = 10,
                    DefaultOffsetY = 20
                },
                Templates = new TemplatesConfig
                {
                    Path = "/etc/printservice/templates"
                }
            };

            var json = JsonSerializer.Serialize(expected, JsonOptions);
            File.WriteAllText(configPath, json);

            // Act
            var actual = Configuration.Load(configPath);

            // Assert
            Assert.Equal(expected.Server.Port, actual.Server.Port);
            Assert.Equal(expected.Server.BindAddress, actual.Server.BindAddress);
            Assert.Equal(expected.Logging.Level, actual.Logging.Level);
            Assert.Equal(expected.Logging.Path, actual.Logging.Path);
            Assert.Equal(expected.Printing.BatchSize, actual.Printing.BatchSize);
            Assert.Equal(expected.Printing.BatchIntervalMs, actual.Printing.BatchIntervalMs);
            Assert.Equal(expected.Printing.DefaultOffsetX, actual.Printing.DefaultOffsetX);
            Assert.Equal(expected.Printing.DefaultOffsetY, actual.Printing.DefaultOffsetY);
            Assert.Equal(expected.Templates.Path, actual.Templates.Path);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Save_WritesConfigToFile()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var configPath = Path.Combine(tempDir, "config.json");

        try
        {
            var config = new Configuration
            {
                Server = new ServerConfig { Port = 9090 }
            };

            config.Save(configPath);

            Assert.True(File.Exists(configPath));

            var json = File.ReadAllText(configPath);
            var loaded = JsonSerializer.Deserialize<Configuration>(json);
            Assert.NotNull(loaded);
            Assert.Equal(9090, loaded.Server.Port);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Load_WhenFileIsInvalid_ThrowsException()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var configPath = Path.Combine(tempDir, "config.json");

        try
        {
            File.WriteAllText(configPath, "this is not valid json");

            Assert.Throws<JsonException>(() => Configuration.Load(configPath));
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }
}
