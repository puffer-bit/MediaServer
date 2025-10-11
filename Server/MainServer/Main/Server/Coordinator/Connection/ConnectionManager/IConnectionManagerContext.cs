using System.Collections.Concurrent;
using Fleck;

namespace Server.MainServer.Main.Server.Coordinator.Connection.Manager;

public interface IConnectionManagerContext
{
    ConcurrentDictionary<string, IClientConnection> ClientConnections { get; }
}