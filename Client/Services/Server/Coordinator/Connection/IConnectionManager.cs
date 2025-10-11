using System.Threading.Tasks;
using Websocket.Client;

namespace Client.Services.Server.Coordinator.Connection;

public interface IConnectionManager
{
    Task<bool> ConnectAsync(string address);
    Task<bool> Reconnect();
    Task Disconnect();
    WebsocketClient? GetWebSocket();
    string? GetAddress();
}