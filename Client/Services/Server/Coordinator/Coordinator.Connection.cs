using System;
using System.Threading;
using System.Threading.Tasks;
using Shared.Models;
using Shared.Models.DTO;
using Shared.Models.Requests;
using Websocket.Client;

namespace Client.Services.Server.Coordinator;

public partial class CoordinatorSession
{
    public async Task ConnectAndAuthenticate(UserDTO user, string address)
    {
        _monitorSubscription?.Dispose();
        ConnectionStatus = CoordinatorState.Connecting;

        var connected = await _connectionManager.ConnectAsync(address);
        if (!connected)
        {
            ConnectionStatus = CoordinatorState.Failed;
            return;
        }
        ConnectionStatus = CoordinatorState.AuthenticationNeeded;

        _messageHandler.StartRouting(_connectionManager.GetWebSocket());
        try
        {
            var status = await _authenticator.AuthenticateAsync(user);
            if (status == UserAuthRequestModel.AuthStatus.Completed)
            {
                ConnectionStatus = CoordinatorState.Connected;
                LastPing = DateTime.UtcNow;
                _monitorSubscription = MonitorServerCommand.Execute().Subscribe();
            }
            else
                ConnectionStatus = CoordinatorState.AuthenticationFailed;
        }
        catch (TimeoutException e)
        {
            ConnectionStatus = CoordinatorState.Failed;
        }
    }

    public async Task ConnectAndAuthenticate(UserDTO user, string address, CancellationToken cancellationToken)
    {
        _monitorSubscription?.Dispose();
        ConnectionStatus = CoordinatorState.Connecting;

        var connected = await _connectionManager.ConnectAsync(address);
        cancellationToken.ThrowIfCancellationRequested();

        if (!connected)
        {
            ConnectionStatus = CoordinatorState.Failed;
            return;
        }

        ConnectionStatus = CoordinatorState.AuthenticationNeeded;
        _messageHandler.StartRouting(_connectionManager.GetWebSocket());

        try
        {
            var status = await _authenticator.AuthenticateAsync(user);
            cancellationToken.ThrowIfCancellationRequested();

            if (status == UserAuthRequestModel.AuthStatus.Completed)
            {
                ConnectionStatus = CoordinatorState.Connected;
                LastPing = DateTime.UtcNow;
                _monitorSubscription = MonitorServerCommand.Execute().Subscribe();
            }
            else
            {
                ConnectionStatus = CoordinatorState.AuthenticationFailed;
            }
        }
        catch (TimeoutException)
        {
            ConnectionStatus = CoordinatorState.Failed;
        }
        catch (OperationCanceledException)
        {
            ConnectionStatus = CoordinatorState.Failed;
        }
    }
    
    public async Task Reconnect()
    {
        _monitorSubscription?.Dispose();
        ConnectionStatus = CoordinatorState.Reconnecting;

        var connected = await _connectionManager.Reconnect();
        if (!connected)
        {
            ConnectionStatus = CoordinatorState.Failed;
            return;
        }
        ConnectionStatus = CoordinatorState.AuthenticationNeeded;
        
        _messageHandler.StartRouting(_connectionManager.GetWebSocket());
        try
        {
            var status = await _authenticator.AuthenticateAsync(GetUser());
            if (status == UserAuthRequestModel.AuthStatus.Completed)
            {
                ConnectionStatus = CoordinatorState.Connected;
                LastPing = DateTime.UtcNow;
                _monitorSubscription = MonitorServerCommand.Execute().Subscribe();
            }
            else
                ConnectionStatus = CoordinatorState.AuthenticationFailed;
        }
        catch (TimeoutException e)
        {
            ConnectionStatus = CoordinatorState.Failed;
        }
    }
    
    public async Task Disconnect()
    {
        _monitorSubscription?.Dispose();
        _monitorSubscription = null;
        await _connectionManager.Disconnect();
        ConnectionStatus = CoordinatorState.Disconnected;
    }

    public void SetCoordinatorInstanceData(string coordinatorInstanceId)
    {
        CoordinatorDTO = new CoordinatorSessionDTO(coordinatorInstanceId, _connectionManager.GetAddress(), GetUser());
    }
    
    public WebsocketClient? GetWebSocket()
    {
        return _connectionManager.GetWebSocket();
    }
}