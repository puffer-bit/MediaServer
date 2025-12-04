using Server.Domain.Entities;
using Server.Domain.Enums;
using Shared.Models;
using Shared.Models.DTO;

namespace Server.MainServer.Main.Server.Coordinator;

public interface ICoordinatorInstance
{
    ICoordinatorInstanceContext Context { get; init; }
    bool IsReady { get; set; }
    bool IsGatewayReady { get; set; }
    bool IsStarted { get; set; }
    CoordinatorState State { get; set; }
    bool IsRemoteConnectionsAvailable { get; set; }
    bool IsRemoteConnectionsRestricted { get; set; }
    bool IsStunServerAvailable { get; set; }
    bool IsTurnServerAvailable { get; set; }
    TimeSpan UpTime => DateTime.UtcNow - Context.CurrentLaunchTime;
    
    event Func<string, Task>? ContextChanged;
    event Func<string, Task>? RequestClose;
    event Func<VideoSessionDTO, Task>? VideoSessionAdded;
    event Func<VideoSessionDTO, Task>? VideoSessionReconfigured;
    event Func<string, Task>? VideoSessionRemoved;
    
    Task Configure(ILoggerFactory loggerFactory, CoordinatorInstanceEntity coordinatorInstanceEntity);
    Task CheckTurnAndStunServer();
    void CloseCoordinator();

    
    void RemoveUserFromInstance(string userId, string reason);
    Task LoadSessions(List<VideoSessionEntity> videoSessionsEntities);
    void SendMessageToUser(string userId, BaseMessage message);

    CoordinatorSessionDTO AsModel();
}