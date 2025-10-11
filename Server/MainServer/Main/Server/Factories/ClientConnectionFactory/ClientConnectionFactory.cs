using Fleck;
using Server.MainServer.Main.Server.Coordinator.Connection;

namespace Server.MainServer.Main.Server.Factories.ClientConnectionFactory;

public class ClientConnectionFactory : IClientConnectionFactory
{
    private readonly IServiceProvider _provider;

    public ClientConnectionFactory(IServiceProvider provider)
    {
        _provider = provider;
    }

    public IClientConnection CreateClientConnection(string userId, IWebSocketConnection webSocket) =>
        ActivatorUtilities.CreateInstance<ClientConnection>(_provider, userId, webSocket, new CancellationTokenSource());
}