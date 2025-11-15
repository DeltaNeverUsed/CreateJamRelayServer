namespace RelayServer.JsonObjects
{
	public class LobbyCreated
	{
		public LobbyCreated(string lobbyCode)
		{
			LobbyCode = lobbyCode;
		}

		public string LobbyCode { get; set; }
	}
}