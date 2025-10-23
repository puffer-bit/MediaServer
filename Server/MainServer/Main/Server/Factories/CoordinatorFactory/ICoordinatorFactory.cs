using Server.MainServer.Main.Server.Coordinator;
using Server.MainServer.Main.Server.Coordinator.Connection;
using Server.MainServer.Main.Server.Coordinator.Connection.Manager;
using Server.MainServer.Main.Server.Coordinator.Manager;
using Server.MainServer.Main.Server.Coordinator.MessagesProcessing.MessageSender.Sender;
using Server.MainServer.Main.Server.Coordinator.MessagesProcessing.MessagesHandlers;
using Server.MainServer.Main.Server.Coordinator.Users.Manager;
using Server.MainServer.Main.Server.Coordinator.WebRTC.Manager;

namespace Server.MainServer.Main.Server.Factories.CoordinatorFactory;

public interface ICoordinatorFactory
{
    void Initialize(CoordinatorInstance coordinator);
    
    ICoordinatorInstanceContext CreateCoordinatorInstanceContext();

    IConnectionManager CreateConnectionManager();
    IConnectionManagerContext CreateConnectionManagerContext();
    
    IUserManager CreateUserManager();
    IUserManagerContext CreateUserManagerContext();
    
    ISessionManager CreateSessionManager();
    ISessionManagerContext CreateSessionManagerContext();
    
    IWebRTCManager CreateWebRTCManager();
    IWebRTCManagerContext CreateWebRTCManagerContext();
    
    IMessageSender CreateMessageSender();

    IHeartbeatManager CreateHeartbeatManager();
    
    IEnumerable<IMessageHandler> CreateMessageHandlers();
}