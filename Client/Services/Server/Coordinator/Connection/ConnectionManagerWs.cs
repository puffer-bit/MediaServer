using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Shared.Models;
using Newtonsoft.Json;
using Websocket.Client;

namespace Client.Services.Server.Coordinator.Connection;

public class ConnectionManagerWs : IConnectionManager
{
    private WebsocketClient? Client { get; set; }
    private string? Address { get; set; }

    // TODO - Add websocket IP address variable
    public async Task<bool> ConnectAsync(string address)
    {
        var factory = new Func<ClientWebSocket>(() =>
        {
            var ws = new ClientWebSocket();
            ws.Options.RemoteCertificateValidationCallback = (_, _, _, _) => true;
            return ws;
        });

        Client = new WebsocketClient(new Uri($"wss://{address}/"), factory);
        Client.IsReconnectionEnabled = false;
        await Client.Start();
        if (Client.IsRunning)
        {
            Address = address;
        }
        return Client.IsRunning;
    }

    public async Task<bool> Reconnect()
    {
        var factory = new Func<ClientWebSocket>(() =>
        {
            var ws = new ClientWebSocket();
            ws.Options.RemoteCertificateValidationCallback = (_, _, _, _) => true;
            return ws;
        });

        Client = new WebsocketClient(new Uri($"wss://{Address}/"), factory);
        Client.IsReconnectionEnabled = false;
        await Client.Start();
        return Client.IsRunning;
    }
    
    public async Task Disconnect()
    {
        if (Client != null && Client.IsRunning)
        {
            await Client.Stop(WebSocketCloseStatus.NormalClosure, "Normal closing");
            Client?.Dispose();
        }
        else
        {
            Client?.Dispose();
        }
    }

    public WebsocketClient? GetWebSocket()
    {
        return Client;
    }

    public string? GetAddress()
    {
        return Address;
    }
}