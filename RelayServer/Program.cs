using System.Net;
using System.Net.WebSockets;

namespace RelayServer;

class Program {
    private static HttpListener _listener = new HttpListener();
    
    private static async Task Main(string[] args) {
        _listener.Prefixes.Add("http://*:8080/");
        _listener.Start();

        while (_listener.IsListening) {
            HttpListenerContext context = await _listener.GetContextAsync();
            if (!context.Request.IsWebSocketRequest)
                continue;
            
            HttpListenerWebSocketContext webSocketContext = await context.AcceptWebSocketAsync(null);
            var connection = new Connection(webSocketContext.WebSocket);
        }
    }
}