using Fleck;
using Server.MainServer.Main.Server.Factories.ClientConnectionFactory;

namespace Server.MainServer.Main.Server.Coordinator.Connection.Manager
{
    public class ConnectionManager : IConnectionManager
    {
        private readonly IConnectionManagerContext _context;
        private readonly IClientConnectionFactory _clientConnectionFactory;
    
        public ConnectionManager(IConnectionManagerContext context, IClientConnectionFactory clientConnectionFactory)
        {
            _context = context;
            _clientConnectionFactory = clientConnectionFactory;
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
            if (_context.ClientConnections.TryGetValue(userId, out var connection))
            {
                connection.TokenSource.Cancel();
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
    }
}