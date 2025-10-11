using System;
using Client.Services.Server.Coordinator;
using Client.Services.Server.Coordinator.Authentication;
using Client.Services.Server.Coordinator.Connection;
using Client.Services.Server.Coordinator.Messaging;
using Client.Services.Server.Coordinator.Messaging.HeartbeatHandler;
using Client.Services.Server.Coordinator.Messaging.MessageRouter;
using Client.Services.Server.Coordinator.Messaging.MessageSender;
using Client.Services.Server.Coordinator.Sessions;
using Client.Services.Server.Coordinator.UserManager;
using Microsoft.Extensions.DependencyInjection;

namespace Client.Services.Server.Factories.CoordinatorFactory;

public class CoordinatorFactory : ICoordinatorFactory
{
    private readonly IServiceProvider _provider;
    private CoordinatorSession? _coordinatorSession;

    public CoordinatorFactory(IServiceProvider provider)
    {
        _provider = provider;
    }

    public void Initialize(CoordinatorSession coordinatorSession)
    {
        _coordinatorSession = coordinatorSession;
    }

    public IAuthenticator CreateAuthenticator(ResponseAwaiter responseAwaiter) =>
        ActivatorUtilities.CreateInstance<Authenticator>(_provider, _coordinatorSession!, responseAwaiter);

    public IConnectionManager CreateConnectionManager() =>
        ActivatorUtilities.CreateInstance<ConnectionManagerWs>(_provider);

    public IUserManager CreateUserManager() =>
        ActivatorUtilities.CreateInstance<UserManager>(_provider);

    public ISessionManager CreateSessionManager(ResponseAwaiter responseAwaiter) =>
        ActivatorUtilities.CreateInstance<SessionManager>(_provider, _coordinatorSession!, responseAwaiter);

    public IMessageHandler CreateMessageRouter(ResponseAwaiter responseAwaiter) =>
        ActivatorUtilities.CreateInstance<MessageHandler>(_provider, _coordinatorSession!);

    public IMessageSender CreateMessageSender() =>
        ActivatorUtilities.CreateInstance<MessageSender>(_provider, _coordinatorSession!);
    
    public IHeartbeatHandler CreateHeartbeatHandler() =>
        ActivatorUtilities.CreateInstance<HeartbeatHandler>(_provider, _coordinatorSession);

    public ResponseAwaiter CreateResponseAwaiter() =>
        ActivatorUtilities.CreateInstance<ResponseAwaiter>(_provider);
}