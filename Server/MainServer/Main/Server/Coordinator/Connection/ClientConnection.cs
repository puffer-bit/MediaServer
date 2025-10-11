using Fleck;

namespace Server.MainServer.Main.Server.Coordinator.Connection;

public class ClientConnection : IClientConnection
{
    public required string UserId { get; set; }
    public required IWebSocketConnection WebSocket { get; set; }
    public required CancellationTokenSource TokenSource { get; set; }
    public required DateTime LastPong { get; set; }
    
    public ClientConnection(string userId, 
        IWebSocketConnection webSocket, 
        CancellationTokenSource tokenSource)
    {
        UserId = userId;
        WebSocket = webSocket;
        TokenSource = tokenSource;
        LastPong = DateTime.UtcNow;
    }
}