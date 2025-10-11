using Fleck;

namespace Server.MainServer.Main.Server.Coordinator.Connection.Manager;

public interface IConnectionManager
{
    CancellationTokenSource Add(string userId, IWebSocketConnection webSocket);
    bool Remove(string userId);
    bool Get(string userId, out IClientConnection? webSocket);
    IDictionary<string, IClientConnection> GetAll();
}