using Server.Domain.Entities;
using Server.MainServer.Main.Server.Coordinator;
using Server.MainServer.Main.Server.Coordinator.Connection;
using Server.MainServer.Main.Server.Coordinator.Connection.ConnectionManager;
using Server.MainServer.Main.Server.Coordinator.Connection.HeartbeatManager;
using Server.MainServer.Main.Server.Coordinator.Messages.MessageSender.Sender;
using Server.MainServer.Main.Server.Coordinator.Messages.MessagesHandlers;
using Server.MainServer.Main.Server.Coordinator.Messages.MessagesHandlers.Heartbeat;
using Server.MainServer.Main.Server.Coordinator.Messages.MessagesHandlers.SessionActions;
using Server.MainServer.Main.Server.Coordinator.Messages.MessagesHandlers.SessionInfo;
using Server.MainServer.Main.Server.Coordinator.Messages.MessagesHandlers.UserInfo;
using Server.MainServer.Main.Server.Coordinator.Messages.MessagesHandlers.WebRTCNegotiation;
using Server.MainServer.Main.Server.Coordinator.Sessions.Manager;
using Server.MainServer.Main.Server.Coordinator.Users.Authenticator;
using Server.MainServer.Main.Server.Coordinator.Users.Manager;
using Server.MainServer.Main.Server.Coordinator.WebRTC.Manager;

namespace Server.MainServer.Main.Server.Factories.CoordinatorFactory;

public class CoordinatorInstanceFactory : ICoordinatorInstanceFactory
{
    private readonly IServiceProvider _provider;

    public CoordinatorInstanceFactory(IServiceProvider provider)
    {
        _provider = provider;
    }

    public CoordinatorInstance CreateCoordinatorInstance(string serverVersion, CoordinatorInstanceEntity coordinatorInstanceEntity) =>
        ActivatorUtilities.CreateInstance<CoordinatorInstance>(_provider, serverVersion, coordinatorInstanceEntity);
    
    public CoordinatorInstanceContext CreateCoordinatorInstanceContext(string coordinatorInstanceId, string serverVersion) =>
        ActivatorUtilities.CreateInstance<CoordinatorInstanceContext>(_provider, serverVersion, coordinatorInstanceId);

    public IConnectionManager CreateConnectionManager(CoordinatorInstance coordinatorInstance) =>
        ActivatorUtilities.CreateInstance<ConnectionManager>(_provider, CreateConnectionManagerContext(), coordinatorInstance);
    private IConnectionManagerContext CreateConnectionManagerContext() =>
        ActivatorUtilities.CreateInstance<ConnectionManagerContext>(_provider);

    public IAuthenticator CreateAuthenticator(CoordinatorInstance coordinatorInstance) =>
        ActivatorUtilities.CreateInstance<Authenticator>(_provider, coordinatorInstance);

    public IUserManager CreateUserManager(CoordinatorInstance coordinatorInstance) =>
        ActivatorUtilities.CreateInstance<UserManager>(_provider, CreateUserManagerContext(), coordinatorInstance);
    private IUserManagerContext CreateUserManagerContext() =>
        ActivatorUtilities.CreateInstance<UserManagerContext>(_provider);

    public ISessionManager CreateSessionManager(CoordinatorInstance coordinatorInstance) =>
        ActivatorUtilities.CreateInstance<SessionManager>(_provider, CreateSessionManagerContext(), coordinatorInstance);
    private ISessionManagerContext CreateSessionManagerContext() =>
        ActivatorUtilities.CreateInstance<SessionManagerContext>(_provider);

    public IWebRTCManager CreateWebRTCManager(CoordinatorInstance coordinatorInstance) =>
        ActivatorUtilities.CreateInstance<WebRTCManager>(_provider, CreateWebRTCManagerContext(), coordinatorInstance);
    private IWebRTCManagerContext CreateWebRTCManagerContext() =>
        ActivatorUtilities.CreateInstance<WebRTCManagerContext>(_provider);

    public IMessageSender CreateMessageSender(CoordinatorInstance coordinatorInstance) =>
        ActivatorUtilities.CreateInstance<MessageSender>(_provider, coordinatorInstance);
    
    public IHeartbeatManager CreateHeartbeatManager(CoordinatorInstance coordinatorInstance) =>
        ActivatorUtilities.CreateInstance<HeartbeatManager>(_provider, coordinatorInstance);

    public IEnumerable<IMessageHandler> CreateMessageHandlers(CoordinatorInstance coordinatorInstance)
    {
        yield return ActivatorUtilities.CreateInstance<SessionActionMessageHandler>(_provider, coordinatorInstance);
        yield return ActivatorUtilities.CreateInstance<SessionInfoRequestMessagesHandler>(_provider, coordinatorInstance);
        yield return ActivatorUtilities.CreateInstance<WebRTCNegotiationMessagesHandler>(_provider, coordinatorInstance);
        yield return ActivatorUtilities.CreateInstance<UserInfoRequestMessagesHandler>(_provider, coordinatorInstance);
        yield return ActivatorUtilities.CreateInstance<HeartbeatHandler>(_provider, coordinatorInstance);
    }
}