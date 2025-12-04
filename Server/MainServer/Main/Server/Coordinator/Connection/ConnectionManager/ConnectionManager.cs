using Fleck;
using Server.MainServer.Main.Server.Factories.ClientConnectionFactory;
using Shared.Enums;
using Shared.Models;
using Shared.Models.Requests.Heartbeat;

namespace Server.MainServer.Main.Server.Coordinator.Connection.ConnectionManager
{
    public class ConnectionManager : IConnectionManager
    {
        private readonly IConnectionManagerContext _context;
        private readonly IClientConnectionFactory _clientConnectionFactory;
        private readonly CoordinatorInstance _coordinator;
    
        public ConnectionManager(IConnectionManagerContext context, IClientConnectionFactory clientConnectionFactory, CoordinatorInstance coordinator)
        {
            _context = context;
            _clientConnectionFactory = clientConnectionFactory;
            _coordinator = coordinator;
        }

        public CancellationTokenSource Add(string userId, IWebSocketConnection socket)
        {
            if (_context.ClientConnections.TryAdd(userId,
                    _clientConnectionFactory.CreateClientConnection(userId, socket)))
            {
                _context.ClientConnections[userId] = _clientConnectionFactory.CreateClientConnection(userId, socket);
            }

            return _context.ClientConnections[userId].TokenSource;
        }
        public bool Remove(string userId)
        {
            if (_context.ClientConnections.TryGetValue(userId, out var clientConnection))
            {
                clientConnection.TokenSource.Cancel();
                clientConnection.Dispose();
            }
            return _context.ClientConnections.TryRemove(userId, out _);
        }
        public bool Get(string userId, out IClientConnection? webSocket)
        {
            if (_context.ClientConnections.TryGetValue(userId, out var socket))
            {
                webSocket = socket;
                return true;
            }
            webSocket = null;
            return false;
        }
        public IDictionary<string, IClientConnection> GetAll() => _context.ClientConnections;

        public void DisconnectAllUsers(bool isRestarting)
        {
            foreach (var clientConnection in _context.ClientConnections.Values)
            {
                clientConnection.TokenSource.Cancel();
                if (isRestarting)
                {
                    _coordinator.SendMessageToUser(clientConnection.UserId,
                        new BaseMessage(MessageType.Heartbeat, new HeartbeatModel(HeartbeatType.ServerRestarting)));
                    _coordinator.RemoveUserFromInstance(clientConnection.UserId, "Server restarting.");
                }
                else
                {
                    _coordinator.SendMessageToUser(clientConnection.UserId,
                        new BaseMessage(MessageType.Heartbeat, new HeartbeatModel(HeartbeatType.ServerClosing)));
                    _coordinator.RemoveUserFromInstance(clientConnection.UserId, "Server closed.");
                }
                
                clientConnection.Dispose();
            }
            
            _context.ClientConnections.Clear();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}