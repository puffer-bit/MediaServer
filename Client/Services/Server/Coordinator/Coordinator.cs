using System;
using System.Collections.Specialized;
using System.Reactive;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Client.Services.Other.AppInfrastructure;
using Client.Services.Server.Coordinator.Authentication;
using Client.Services.Server.Coordinator.Connection;
using Client.Services.Server.Coordinator.Messaging;
using Client.Services.Server.Coordinator.Messaging.HeartbeatHandler;
using Client.Services.Server.Coordinator.Messaging.MessageRouter;
using Client.Services.Server.Coordinator.Messaging.MessageSender;
using Client.Services.Server.Coordinator.Sessions;
using Client.Services.Server.Coordinator.UserManager;
using Client.Services.Server.Factories.CoordinatorFactory;
using ReactiveUI;
using Shared.Models;

namespace Client.Services.Server.Coordinator;

public partial class CoordinatorSession : ReactiveObject, ICoordinatorSession
{
    private CoordinatorState _connectionStatus = CoordinatorState.New;
    public CoordinatorState ConnectionStatus
    {
        get => _connectionStatus;
        set => this.RaiseAndSetIfChanged(ref _connectionStatus, value);
    }
    
    public DateTime? LastPing { get; set; }
    public CancellationTokenSource Cts { get; set; }
    public CoordinatorSessionDTO? CoordinatorDTO { get; set; }
    public event Action? OnDisconnect;
    private ReactiveCommand<Unit, Unit> MonitorServerCommand { get; }
    private IDisposable? _monitorSubscription;
    
    public event NotifyCollectionChangedEventHandler? SessionsCollectionChanged;

    private readonly IConnectionManager _connectionManager;
    private readonly IAuthenticator _authenticator;
    private readonly IMessageHandler _messageHandler;
    private readonly ISessionManager _sessionsManager;
    private readonly IMessageSender _messageSender;
    private readonly IUserManager _userManager;
    private readonly IHeartbeatHandler _heartbeatHandler;
    private readonly ResponseAwaiter _responseAwaiter;
    private readonly AppSettingsManager _appSettingsManager;

    public CoordinatorSession(ICoordinatorFactory coordinatorFactory, 
        AppSettingsManager appSettingsManager)
    {
        // Fabric
        coordinatorFactory.Initialize(this);
        _responseAwaiter = coordinatorFactory.CreateResponseAwaiter();
        _connectionManager = coordinatorFactory.CreateConnectionManager();
        _authenticator = coordinatorFactory.CreateAuthenticator(_responseAwaiter);
        _messageHandler = coordinatorFactory.CreateMessageRouter(_responseAwaiter);
        _sessionsManager = coordinatorFactory.CreateSessionManager(_responseAwaiter);
        _messageSender = coordinatorFactory.CreateMessageSender();
        _userManager = coordinatorFactory.CreateUserManager();
        _heartbeatHandler = coordinatorFactory.CreateHeartbeatHandler();
        
        // DI
        _appSettingsManager = appSettingsManager;
        
        // Local
        Cts = new CancellationTokenSource();
        MonitorServerCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            while (!Cts.Token.IsCancellationRequested)
            {
                if (!_heartbeatHandler.IsServerAlive())
                {
                    if (ConnectionStatus == CoordinatorState.Connected)
                        ConnectionStatus = CoordinatorState.HeartbeatTimedOut;
                }

                await Task.Delay(TimeSpan.FromSeconds(5), Cts.Token);
            }
        });
    }

    public void Dispose()
    {
        _monitorSubscription?.Dispose();
        Cts.Dispose();
        MonitorServerCommand.Dispose();
    }
}
