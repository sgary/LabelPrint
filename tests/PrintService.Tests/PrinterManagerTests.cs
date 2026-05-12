using System.Text.Json;
using PrintService.Printing;
using Xunit;

namespace PrintService.Tests;

public class PrinterManagerTests
{
    [Fact]
    public void OffsetManager_SetOffsetAndGetOffset_PersistsToJsonFile()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var configPath = Path.Combine(tempDir, "offsets.json");

        try
        {
            var manager = new OffsetManager(configPath);
            manager.SetOffset("Zebra GK420d", "template-1", 5, -3);

            var reloaded = new OffsetManager(configPath);
            var offset = reloaded.GetOffset("Zebra GK420d", "template-1");

            Assert.Equal(5, offset.X);
            Assert.Equal(-3, offset.Y);

            var json = File.ReadAllText(configPath);
            using var doc = JsonDocument.Parse(json);
            var node = doc.RootElement.GetProperty("Zebra GK420d:template-1");
            Assert.Equal(5, node.GetProperty("x").GetInt32());
            Assert.Equal(-3, node.GetProperty("y").GetInt32());
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public void OffsetManager_Calibrate_ComputesAndStoresDelta()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var configPath = Path.Combine(tempDir, "offsets.json");

        try
        {
            var manager = new OffsetManager(configPath);
            manager.Calibrate("Brother QL", "template-2", measuredX: 102, measuredY: 97, expectedX: 100, expectedY: 100);

            var offset = manager.GetOffset("Brother QL", "template-2");
            Assert.Equal(-2, offset.X);
            Assert.Equal(3, offset.Y);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public void PrinterManager_GetCommandGenerator_DetectsZebraAndBrother()
    {
        var manager = new PrinterManager(new OffsetManager(Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json")));

        var zebra = manager.GetCommandGenerator("Warehouse Zebra ZT230");
        var brother = manager.GetCommandGenerator("Brother QL-820NWB");

        Assert.Equal("ZplGenerator", zebra.GetType().Name);
        Assert.Equal("BplGenerator", brother.GetType().Name);
    }

    [Fact]
    public async Task PrintAsync_AppliesOffsetAndEmitsGeneratedCommands()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var configPath = Path.Combine(tempDir, "offsets.json");

        try
        {
            var offsetManager = new OffsetManager(configPath);
            offsetManager.SetOffset("Zebra GK420d", "template-3", 10, 20);
            var manager = new PrinterManager(offsetManager);

            var job = new PrintJob
            {
                RequestId = Guid.NewGuid(),
                TemplateId = "template-3",
                Data = new Dictionary<string, object>
                {
                    ["name"] = "Alice"
                },
                Options = new PrintOptions
                {
                    PrinterName = "Zebra GK420d",
                    Copies = 1,
                    OffsetX = 2,
                    OffsetY = -5
                }
            };

            var template = new PrintTemplate
            {
                Id = "template-3",
                Name = "Label",
                Version = "1.0"
            };

            string? generated = null;
            await manager.PrintAsync(job, template, batchNumber: 1, onZplGenerated: commands => generated = commands);

            Assert.False(string.IsNullOrWhiteSpace(generated));
            Assert.Contains("^XA", generated);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }
}
