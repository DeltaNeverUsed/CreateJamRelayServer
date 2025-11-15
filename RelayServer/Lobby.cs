using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using RelayServer.JsonObjects;

namespace RelayServer;

public class Lobby : IDisposable {
    public void Dispose() {
        foreach (Connection client in _clients) {
            client.CloseConnection();
        }
        _clients.Clear();
        _host = null;
        Lobbies.Remove(LobbyCode);
    }

    public static Dictionary<string, Lobby> Lobbies = new Dictionary<string, Lobby>();
    private const string ValidChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private const int CodeLength = 4;

    private Connection _host;
    private List<Connection> _clients = new List<Connection>();
    public readonly string LobbyCode;

    public void AddClient(Connection client, string playerName) {
        _clients.Add(client);
        client.OnMessage += ms => { _host.Send(ms.ToArray(), WebSocketMessageType.Text); };
        var clientConnection = new ClientConnectionResponse(playerName);
        _host.Send(clientConnection);
        client.Send(clientConnection);
    }

    public void RemoveClient(Connection client) {
        _clients.Remove(client);
    }

    public void Broadcast(ArraySegment<byte> buff) {
        foreach (Connection client in _clients) {
            _ = client.Send(buff, WebSocketMessageType.Text);
        }
    }

    private string CreateLobbyCode() {
        StringBuilder sb = new();
        for (int i = 0; i < CodeLength; i++) {
            sb.Append(ValidChars[Random.Shared.Next(ValidChars.Length)]);
        }

        return sb.ToString();
    }

    public Lobby(Connection host) {
        _host = host;

        do {
            LobbyCode = CreateLobbyCode();
        } while (Lobbies.ContainsKey(LobbyCode));

        Lobbies.Add(LobbyCode, this);
        _host.OnMessage += ms => { Broadcast(ms.ToArray()); };
        
        _host.Send(new LobbyCreated(LobbyCode));
    }
}