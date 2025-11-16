namespace RelayServer.JsonObjects
{
    public class NetworkMessage
    {
        public MessageType MessageId;
        public string Json;
    }
	
    public enum MessageType
    {
        GameState = 0,
        CreateTradeOffer = 1,
        AcceptTradeOffer = 2,
        PizzaChange = 3,
        LobbyCreated = 4,
        ClientConnectionResponse = 5,
        ControlMessage = 6,
    }
}