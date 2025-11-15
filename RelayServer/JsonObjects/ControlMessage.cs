namespace RelayServer.JsonObjects
{
	public class ControlMessage
	{
		public enum MessageType {
			Host,
			Connect
		}

		public MessageType Type { get; set; }
		public string? LobbyCode { get; set; }
		public string? PlayerName { get; set; }
	}
}