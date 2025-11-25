using System.Collections.Concurrent;

namespace Server.MainServer.Main.Server.Coordinator.Connection.ConnectionManager;

public interface IConnectionManagerContext : IDisposable
{
    ConcurrentDictionary<string, IClientConnection> ClientConnections { get; }
}