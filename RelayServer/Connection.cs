using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using RelayServer.JsonObjects;

namespace RelayServer;

public class Connection {
    private WebSocket _socket;
    private readonly CancellationTokenSource _cts = new();

    private bool _ready = false;
    private Lobby? _lobby;

    private bool _isHost;

    public delegate void OnMessageCallback(MemoryStream ms);

    public OnMessageCallback OnMessage;

    public Connection(WebSocket socket) {
        _socket = socket;
        _ = Receive();
    }

    public async Task CloseConnection() {
        await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
        _cts.Cancel();
        _socket.Abort();
        _socket.Dispose();
    }

    public async Task Send<T>(MessageType type, T jsonObject) {
        var msg = new NetworkMessage() {
            MessageId = type,
            Json = JsonSerializer.Serialize(jsonObject, JsonSerializerOptions.Default)
        };
        await Send(msg);
    }

    public async Task Send<T>(T jsonObject) {
        string serialized = JsonSerializer.Serialize(jsonObject, JsonSerializerOptions.Default);
        await Send(Encoding.UTF8.GetBytes(serialized), WebSocketMessageType.Text);
    }

    public async Task Send(ArraySegment<byte> buff, WebSocketMessageType type) {
        await _socket.SendAsync(buff, type, true, _cts.Token);
    }

    public void ParseControlMessage(MemoryStream ms) {
        try {
            var controlMessage = JsonSerializer.Deserialize<ControlMessage>(ms);
            if (controlMessage == null)
                return;

            switch (controlMessage.Type) {
                case ControlMessage.MessageType.Host:
                    _lobby = new Lobby(this);
                    _isHost = true;
                    _ready = true;
                    Console.WriteLine("Host connected and got lobby code " + _lobby.LobbyCode);
                    break;
                case ControlMessage.MessageType.Connect:
                    if (controlMessage.LobbyCode == null) {
                        Console.WriteLine("Got client connect request, but lobby code was empty");
                        return;
                    }

                    if (controlMessage.PlayerName == null || string.IsNullOrWhiteSpace(controlMessage.PlayerName)) {
                        Console.WriteLine("Got client connect request, but PlayerName was empty");
                        return;
                    }

                    if (!Lobby.Lobbies.TryGetValue(controlMessage.LobbyCode.ToUpper(), out _lobby)) {
                        Console.WriteLine("Got client connect request, but lobby code was not found");
                        return;
                    }

                    _lobby.AddClient(this, controlMessage.PlayerName);
                    _ready = true;
                    Console.WriteLine($"Client {controlMessage.PlayerName} connected to lobby {_lobby.LobbyCode}");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        catch (Exception e) {
            //Console.WriteLine(e);
        }
    }

    private void ConnectionClosed() {
        if (_isHost)
            _lobby?.Dispose();
        else
            _lobby?.RemoveClient(this);
        Console.WriteLine("Connection closed");
    }

    public async Task Receive() {
        ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[8192]);

        while (_socket.State == WebSocketState.Open) {
            try {
                using var ms = new MemoryStream();
                WebSocketReceiveResult result;
                do {
                    result = await _socket.ReceiveAsync(buffer, _cts.Token);
                    if (result.MessageType == WebSocketMessageType.Close)
                        break;
                    ms.Write(buffer.Array, buffer.Offset, result.Count);
                } while (!result.EndOfMessage);

                ms.Seek(0, SeekOrigin.Begin);

                if (!_ready) {
                    ParseControlMessage(ms);
                }
                else {
                    OnMessage(ms);
                }
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }
        }

        ConnectionClosed();
    }
}