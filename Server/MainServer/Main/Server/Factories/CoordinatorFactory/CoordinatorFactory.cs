using Server.MainServer.Main.Server.Coordinator;
using Server.MainServer.Main.Server.Coordinator.Connection;
using Server.MainServer.Main.Server.Coordinator.Connection.Manager;
using Server.MainServer.Main.Server.Coordinator.Manager;
using Server.MainServer.Main.Server.Coordinator.MessagesProcessing.MessageSender.Sender;
using Server.MainServer.Main.Server.Coordinator.MessagesProcessing.MessagesHandlers;
using Server.MainServer.Main.Server.Coordinator.MessagesProcessing.MessagesHandlers.Heartbeat;
using Server.MainServer.Main.Server.Coordinator.MessagesProcessing.MessagesHandlers.SessionInfo;
using Server.MainServer.Main.Server.Coordinator.MessagesProcessing.MessagesHandlers.WebRTCNegotiation;
using Server.MainServer.Main.Server.Coordinator.Users.Manager;
using Server.MainServer.Main.Server.Coordinator.WebRTC.Manager;
using Server.SFU.Main.Server.Coordinator.MessagesProcessing.MessagesHandlers.SessionActions;

namespace Server.MainServer.Main.Server.Factories.CoordinatorFactory;

public class CoordinatorFactory : ICoordinatorFactory
{
    private readonly IServiceProvider _provider;
    private CoordinatorInstance _coordinator;

    public CoordinatorFactory(IServiceProvider provider)
    {
        _provider = provider;
    }

    public void Initialize(CoordinatorInstance coordinator)
    {
        _coordinator = coordinator;
    }

    public IConnectionManager CreateConnectionManager() =>
        ActivatorUtilities.CreateInstance<ConnectionManager>(_provider, CreateConnectionManagerContext());
    public IConnectionManagerContext CreateConnectionManagerContext() =>
        ActivatorUtilities.CreateInstance<ConnectionManagerContext>(_provider);

    public IUserManager CreateUserManager() =>
        ActivatorUtilities.CreateInstance<UserManager>(_provider, CreateUserManagerContext(), _coordinator);
    public IUserManagerContext CreateUserManagerContext() =>
        ActivatorUtilities.CreateInstance<UserManagerContext>(_provider);

    public ISessionManager CreateSessionManager() =>
        ActivatorUtilities.CreateInstance<SessionManager>(_provider, CreateSessionManagerContext(), _coordinator);
    public ISessionManagerContext CreateSessionManagerContext() =>
        ActivatorUtilities.CreateInstance<SessionManagerContext>(_provider);

    public IWebRTCManager CreateWebRTCManager() =>
        ActivatorUtilities.CreateInstance<WebRTCManager>(_provider, CreateWebRTCManagerContext(), _coordinator);
    public IWebRTCManagerContext CreateWebRTCManagerContext() =>
        ActivatorUtilities.CreateInstance<WebRTCManagerContext>(_provider);

    public IMessageSender CreateMessageSender() =>
        ActivatorUtilities.CreateInstance<MessageSender>(_provider, _coordinator);
    
    public IHeartbeatManager CreateHeartbeatManager() =>
        ActivatorUtilities.CreateInstance<HeartbeatManager>(_provider, _coordinator);

    public IEnumerable<IMessageHandler> CreateMessageHandlers()
    {
        yield return ActivatorUtilities.CreateInstance<SessionActionMessageHandler>(_provider, _coordinator);
        yield return ActivatorUtilities.CreateInstance<InfoRequestMessagesHandler>(_provider, _coordinator);
        yield return ActivatorUtilities.CreateInstance<WebRTCNegotiationMessagesHandler>(_provider, _coordinator);
        yield return ActivatorUtilities.CreateInstance<HeartbeatHandler>(_provider, _coordinator);
    }
}