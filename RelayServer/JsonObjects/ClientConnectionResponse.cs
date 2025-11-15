namespace RelayServer.JsonObjects
{
	public class ClientConnectionResponse
	{
		public string ClientName { get; set; }

		public ClientConnectionResponse(string clientName)
		{
			ClientName = clientName;
		}
	}
}