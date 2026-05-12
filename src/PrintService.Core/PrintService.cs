using System.ServiceProcess;
using PrintService.Printing;
using PrintService.Protocol;

namespace PrintService.Core;

public class PrintService : ServiceBase
{
    private readonly string _baseDataPath;
    private readonly string _configPath;
    private readonly string _templatesPath;
    private readonly string _logsPath;
    private readonly string _logFilePath;

    private Configuration _configuration = new();
    private readonly Dictionary<string, PrintTemplate> _templates = new(StringComparer.OrdinalIgnoreCase);
    private OffsetManager? _offsetManager;
    private PrinterManager? _printerManager;
    private PrintQueue? _printQueue;
    private MessageHandler? _messageHandler;
    private WebSocketServer? _webSocketServer;

    public PrintService()
    {
        ServiceName = "PrintService";

        _baseDataPath = @"C:\ProgramData\PrintService";
        _configPath = Path.Combine(_baseDataPath, "config.json");
        _templatesPath = Path.Combine(_baseDataPath, "templates");
        _logsPath = Path.Combine(_baseDataPath, "logs");
        _logFilePath = Path.Combine(_logsPath, "service.log");
    }

    protected override void OnStart(string[] args)
    {
        try
        {
            _configuration = Configuration.Load(_configPath);

            EnsureDirectories();
            LoadTemplates();

            _offsetManager = new OffsetManager(_configuration.Printing.DefaultOffsetX, _configuration.Printing.DefaultOffsetY);
            _printerManager = new PrinterManager();
            _printQueue = new PrintQueue(_configuration.Printing.BatchSize, _configuration.Printing.BatchIntervalMs);

            _messageHandler = new MessageHandler(
                _printerManager,
                _printQueue,
                templateId => _templates.TryGetValue(templateId, out var template) ? template : null,
                SendProgressToAllAsync);

            var url = $"ws://{_configuration.Server.BindAddress}:{_configuration.Server.Port}";
            _webSocketServer = new WebSocketServer(url, _messageHandler);
            _webSocketServer.Start();

            LogInfo("PrintService started successfully");
        }
        catch (Exception ex)
        {
            LogError($"Startup failed: {ex}");
            throw;
        }
    }

    protected override void OnStop()
    {
        try
        {
            _webSocketServer?.Stop();
            _webSocketServer = null;

            _printQueue?.Clear();

            LogInfo("PrintService stopped");
        }
        catch (Exception ex)
        {
            LogError($"Stop failed: {ex}");
        }
    }

    public void StartConsole(string[] args)
    {
        OnStart(args);
    }

    public void StopConsole()
    {
        OnStop();
    }

    private void EnsureDirectories()
    {
        Directory.CreateDirectory(_baseDataPath);
        Directory.CreateDirectory(_templatesPath);
        Directory.CreateDirectory(_logsPath);
    }

    private void LoadTemplates()
    {
        _templates.Clear();

        foreach (var filePath in Directory.GetFiles(_templatesPath, "*.json", SearchOption.TopDirectoryOnly))
        {
            try
            {
                var template = TemplateEngine.LoadTemplate(filePath);
                if (!string.IsNullOrWhiteSpace(template.Id))
                {
                    _templates[template.Id] = template;
                }
            }
            catch (Exception ex)
            {
                LogError($"Failed to load template '{filePath}': {ex.Message}");
            }
        }
    }

    private Task SendProgressToAllAsync(string message)
    {
        return Task.CompletedTask;
    }

    private void LogInfo(string message)
    {
        Log("INFO", message);
    }

    private void LogError(string message)
    {
        Log("ERROR", message);
    }

    private void Log(string level, string message)
    {
        try
        {
            Directory.CreateDirectory(_logsPath);
            var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}{Environment.NewLine}";
            File.AppendAllText(_logFilePath, line);
        }
        catch
        {
            // ignore logging failures
        }
    }
}
