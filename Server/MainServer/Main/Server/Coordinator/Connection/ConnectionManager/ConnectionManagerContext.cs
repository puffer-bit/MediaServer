using System.Collections.Concurrent;
using Fleck;

namespace Server.MainServer.Main.Server.Coordinator.Connection.Manager
{
    public class ConnectionManagerContext : IConnectionManagerContext
    {
        public ConcurrentDictionary<string, IClientConnection> ClientConnections { get; } = new();
    }
}