using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Fleck;
using Newtonsoft.Json.Linq;
using Server.MainServer.Main.Server.Coordinator.Connection;
using Server.MainServer.Main.Server.Coordinator.WebSocket;
using Shared.Enums;
using Shared.Models;
using Shared.Models.DTO;
using Shared.Models.Requests;
using Shared.Models.Requests.Heartbeat;

namespace Server.MainServer.Main.Server.Coordinator;

public partial class CoordinatorInstance
{
    private void CreateWebSocketServer(ILoggerFactory loggerFactory)
    {
        var cert = X509CertificateLoader.LoadPkcs12FromFile("Certs/server.pfx", "MyPassword");
        
        if (string.IsNullOrWhiteSpace(Context.Ip) || Context.Port == 0)
        {
            _webSocketServer = new WebSocketServer("wss://127.0.0.1:26666/");
        }
        else if (IPAddress.TryParse(Context.Ip, out _))
        {
            _webSocketServer = new WebSocketServer($"wss://{Context.Ip}:{Context.Port}/");
        }
        else
        {
            _logger.LogWarning("Failed to parse IP address.");
            _webSocketServer = new WebSocketServer("wss://127.0.0.1:26666/");
        }

        _webSocketServer.EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
        _webSocketServer.Certificate = cert;

        _webSocketServer.Start(socket =>
        {
            var handler = new CoordinatorInstanceWebSocketGateway(this, socket, loggerFactory);

            socket.OnOpen = handler.OnOpen;
            socket.OnClose = handler.OnClose;
            socket.OnMessage = handler.OnMessage;
            socket.OnError = handler.OnError;
        });

        IsGatewayReady = true;
        _logger.LogInformation("Coordinator {Id} WebSocket server listening on IP:{Ip} and Port:{Port}.", Context.Id, Context.Ip, Context.Port);
    }
    
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