using Fleck;
using PrintService.Protocol;
using System.Text.Json;

namespace PrintService.Core;

public class WebSocketServer
{
    private readonly string _url;
    private readonly IMessageHandler _messageHandler;
    private Fleck.WebSocketServer? _server;
    private readonly List<IWebSocketConnection> _clients = new();
    private readonly object _lock = new();

    public WebSocketServer(string url, IMessageHandler messageHandler)
    {
        _url = url;
        _messageHandler = messageHandler;
    }

    public static WebSocketServer CreateDefault(IMessageHandler messageHandler)
    {
        return new WebSocketServer("ws://127.0.0.1:8080", messageHandler);
    }

    public IReadOnlyList<IWebSocketConnection> Clients
    {
        get { lock (_lock) { return _clients.ToList(); } }
    }

    public void Start()
    {
        _server = new Fleck.WebSocketServer(_url);

        _server.Start(socket =>
        {
            socket.OnOpen = () =>
            {
                if (!IsLocalConnection(socket))
                {
                    socket.Close();
                    return;
                }

                lock (_lock)
                {
                    _clients.Add(socket);
                }
            };

            socket.OnClose = () =>
            {
                lock (_lock)
                {
                    _clients.Remove(socket);
                }
            };

            socket.OnMessage = async message =>
            {
                var response = await _messageHandler.HandleMessageAsync(message);
                socket.Send(response);
            };
        });
    }

    public void Stop()
    {
        _server?.Dispose();
        _server = null;

        lock (_lock)
        {
            _clients.Clear();
        }
    }

    private static bool IsLocalConnection(IWebSocketConnection socket)
    {
        if (socket.ConnectionInfo.ClientIpAddress == null)
            return false;

        return socket.ConnectionInfo.ClientIpAddress == "127.0.0.1" ||
               socket.ConnectionInfo.ClientIpAddress == "::1" ||
               socket.ConnectionInfo.ClientIpAddress == "localhost";
    }
}
