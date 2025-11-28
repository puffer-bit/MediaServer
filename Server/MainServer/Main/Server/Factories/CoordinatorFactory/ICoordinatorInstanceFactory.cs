using Server.Domain.Entities;
using Server.MainServer.Main.Server.Coordinator;
using Server.MainServer.Main.Server.Coordinator.Connection;
using Server.MainServer.Main.Server.Coordinator.Connection.ConnectionManager;
using Server.MainServer.Main.Server.Coordinator.Connection.HeartbeatManager;
using Server.MainServer.Main.Server.Coordinator.Messages.MessageSender.Sender;
using Server.MainServer.Main.Server.Coordinator.Messages.MessagesHandlers;
using Server.MainServer.Main.Server.Coordinator.Sessions.Manager;
using Server.MainServer.Main.Server.Coordinator.Users.Authenticator;
using Server.MainServer.Main.Server.Coordinator.Users.Manager;
using Server.MainServer.Main.Server.Coordinator.WebRTC.Manager;
using Server.MainServer.Main.Server.Coordinator.WebSocket;

namespace Server.MainServer.Main.Server.Factories.CoordinatorFactory;

public interface ICoordinatorInstanceFactory
{
    CoordinatorInstance CreateCoordinatorInstance(string serverVersion, CoordinatorInstanceEntity coordinatorInstanceEntity);
    CoordinatorInstanceContext CreateCoordinatorInstanceContext(string coordinatorInstanceId, string serverVersion);

    //CoordinatorWebSocketInstance CreateCoordinatorWebSocketInstance(ICoordinatorInstance coordinatorInstance);
    
    IConnectionManager CreateConnectionManager(CoordinatorInstance coordinatorInstance);
    
    IAuthenticator CreateAuthenticator(CoordinatorInstance coordinatorInstance);
    
    IUserManager CreateUserManager(CoordinatorInstance coordinatorInstance);
    
    ISessionManager CreateSessionManager(CoordinatorInstance coordinatorInstance);
    
    IWebRTCManager CreateWebRTCManager(CoordinatorInstance coordinatorInstance);
    
    IMessageSender CreateMessageSender(CoordinatorInstance coordinatorInstance);

    IHeartbeatManager CreateHeartbeatManager(CoordinatorInstance coordinatorInstance);
    
    IEnumerable<IMessageHandler> CreateMessageHandlers(CoordinatorInstance coordinatorInstance);
}