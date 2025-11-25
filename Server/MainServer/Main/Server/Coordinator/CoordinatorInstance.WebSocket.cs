using Fleck;
using Newtonsoft.Json.Linq;
using Server.MainServer.Main.Server.Coordinator.Connection;
using Shared.Enums;
using Shared.Models;
using Shared.Models.DTO;
using Shared.Models.Requests;
using Shared.Models.Requests.Heartbeat;

namespace Server.MainServer.Main.Server.Coordinator;

public partial class CoordinatorInstance
{
    public CancellationTokenSource AddNewConnection(string userId, IWebSocketConnection webSocket)
    {
        return _connectionManager.Add(userId, webSocket);
    }
    
    public void RemoveConnection(string userId)
    {
        _connectionManager.Remove(userId);
    }

    public bool GetClientConnection(string userId, out IClientConnection? foundedClientConnection)
    {
        if (_connectionManager.Get(userId, out var clientConnection))
        {
            foundedClientConnection = clientConnection!;
            return true;
        }

        foundedClientConnection = null;
        return false;
    }

    public IDictionary<string, IClientConnection> GetAllConnections()
    {
        return _connectionManager.GetAll();
    }

    public void RegisterPong(string userId)
    {
        _heartbeatManager.RegisterPong(userId);
    }
}