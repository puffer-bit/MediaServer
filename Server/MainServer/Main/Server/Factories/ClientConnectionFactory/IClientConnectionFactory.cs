using Fleck;
using Server.MainServer.Main.Server.Coordinator.Connection;

namespace Server.MainServer.Main.Server.Factories.ClientConnectionFactory;

public interface IClientConnectionFactory
{
    IClientConnection CreateClientConnection(string userId, IWebSocketConnection webSocket);
}