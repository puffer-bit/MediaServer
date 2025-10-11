using Fleck;
using Shared.Models;

namespace Server.MainServer.Main.Server.Coordinator.Connection;

public interface IClientConnection
{
    string UserId { get; set; }
    IWebSocketConnection WebSocket { get; set; }
    public CancellationTokenSource TokenSource { get; set; }
    public DateTime LastPong { get; set; }
}