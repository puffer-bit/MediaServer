using System.Collections.Generic;
using Client.Services.Interfaces;
using Client.Services.Server.Coordinator;
using Client.Services.Server.Coordinator.Authentication;
using Client.Services.Server.Coordinator.Connection;
using Client.Services.Server.Coordinator.Messaging;
using Client.Services.Server.Coordinator.Messaging.HeartbeatHandler;
using Client.Services.Server.Coordinator.Messaging.MessageRouter;
using Client.Services.Server.Coordinator.Messaging.MessageSender;
using Client.Services.Server.Coordinator.Sessions;
using Client.Services.Server.Coordinator.UserManager;
using IMessageHandler = Client.Services.Server.Coordinator.Messaging.MessageRouter.IMessageHandler;

namespace Client.Services.Server.Factories.CoordinatorFactory;

public interface ICoordinatorFactory
{
    void Initialize(CoordinatorSession coordinator);

    IAuthenticator CreateAuthenticator(ResponseAwaiter responseAwaiter);
    IConnectionManager CreateConnectionManager();
    IUserManager CreateUserManager();
    ISessionManager CreateSessionManager(ResponseAwaiter responseAwaiter);
    IMessageHandler CreateMessageRouter(ResponseAwaiter responseAwaiter);
    IMessageSender CreateMessageSender();
    IHeartbeatHandler CreateHeartbeatHandler();
    ResponseAwaiter CreateResponseAwaiter();
}