using Shared.Enums;
using Shared.Models;
using Shared.Models.Requests.SessionInfo;

namespace Server.MainServer.Main.Server.Coordinator;

public partial class CoordinatorInstance
{
    public void SessionCreated(string sessionId)
    {
        if (GetVideoSession(sessionId, out var session) == SessionRequestResult.NoError)
        {
            BroadcastMessage(new BaseMessage()
            {
                Data = new SessionStateChanged(session!.AsModel(), SessionStateChangedType.SessionCreated),
                Type = MessageType.SessionsStateChanged
            });
            _logger.LogInformation("SessionCreated fired");
        }
        else
            _logger.LogDebug("Failed to broadcast about new session. (Session not exist)");
    }
    
    public void SessionDeleted(SessionDTO sessionDTO)
    {
        BroadcastMessage(new BaseMessage()
        {
            Data = new SessionStateChanged(sessionDTO, SessionStateChangedType.SessionDeleted),
            Type = MessageType.SessionsStateChanged
        });
        _logger.LogInformation("SessionDeleted fired");
    }
    
    public void SessionReconfigured(string sessionId)
    {
        if (GetVideoSession(sessionId, out var session) == SessionRequestResult.NoError)
        {
            BroadcastMessage(new BaseMessage()
            {
                Data = new SessionStateChanged(session!.AsModel(), SessionStateChangedType.SessionReconfigured),
                Type = MessageType.SessionsStateChanged
            });
            _logger.LogInformation("SessionReconfigured fired");
        }
        else
            _logger.LogDebug("Failed to broadcast about session reconfiguration. (Session not exist)");
    }
}