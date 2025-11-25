using System.Collections.Concurrent;

namespace Server.MainServer.Main.Server.Coordinator.Connection.ConnectionManager
{
    public class ConnectionManagerContext : IConnectionManagerContext
    {
        public ConcurrentDictionary<string, IClientConnection> ClientConnections { get; } = new();

        public void Dispose()
        {
            foreach (var clientConnection in ClientConnections.Values)
            {
                clientConnection.Dispose();
            }
        }
    }
}