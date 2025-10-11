using System;
using Shared.Enums;
using Shared.Models;
using Shared.Models.Requests.Heartbeat;

namespace Client.Services.Server.Coordinator.Messaging.HeartbeatHandler;

public class HeartbeatHandler : IHeartbeatHandler
{
    private readonly CoordinatorSession _coordinatorSession;

    public HeartbeatHandler(CoordinatorSession coordinatorSession)
    {
        _coordinatorSession = coordinatorSession;
    }
    
    public void ReactOnPing()
    {
        _coordinatorSession.LastPing = DateTime.UtcNow;
        _coordinatorSession.SendMessage(new BaseMessage(_coordinatorSession.GetUser().Id, MessageType.Heartbeat, new HeartbeatModel(HeartbeatType.Pong)));
    }
    
    public bool IsServerAlive()
    {
        return DateTime.UtcNow - _coordinatorSession.LastPing <= TimeSpan.FromSeconds(25);
    }
}