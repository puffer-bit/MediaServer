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
    
    public void UserJoinedVideoSession(string peerId, string videoSessionId)
    {
        if (GetVideoSession(videoSessionId, out var session) == SessionRequestResult.NoError)
        {
            BroadcastMessage(new BaseMessage()
            {
                Data = new SessionStateChanged(session!.AsModel(), SessionStateChangedType.UserConnected),
                Type = MessageType.SessionsStateChanged
            });
            _logger.LogInformation("UserJoined fired");
        }
        else
            _logger.LogDebug("Failed to broadcast about UserJoined. (Session not exist)");
    }
    
    public void UserApprovedVideoSession(string peerId, string videoSessionId)
    {
        if (GetVideoSession(videoSessionId, out var session) == SessionRequestResult.NoError)
        {
            session?.ChangePeerApproveState(peerId, VideoSessionApproveState.Approved);
            
            BroadcastMessage(new BaseMessage()
            {
                Data = new SessionStateChanged(session!.AsModel(), SessionStateChangedType.UserApproved),
                Type = MessageType.SessionsStateChanged
            });
            _logger.LogInformation("UserApproved fired");
        }
        else
            _logger.LogDebug("Failed to broadcast about UserApproved. (Session not exist)");
    }
    
    public void UserRejectedVideoSession(string peerId, string videSessionId)
    {
        if (GetVideoSession(videSessionId, out var session) == SessionRequestResult.NoError)
        {
            session?.ChangePeerApproveState(peerId, VideoSessionApproveState.Rejected);
            
            BroadcastMessage(new BaseMessage()
            {
                Data = new SessionStateChanged(session!.AsModel(), SessionStateChangedType.UserRejected),
                Type = MessageType.SessionsStateChanged
            });
            _logger.LogInformation("UserRejected fired");
        }
        else
            _logger.LogDebug("Failed to broadcast about UserRejected. (Session not exist)");
    }
    
    public void UserNegotiatedVideoSession(string peerId, string videoSessionId)
    {
        if (GetVideoSession(videoSessionId, out var session) == SessionRequestResult.NoError)
        {
            session?.ChangePeerApproveState(peerId, VideoSessionApproveState.Connected);
            
            BroadcastMessage(new BaseMessage()
            {
                Data = new SessionStateChanged(session!.AsModel(), SessionStateChangedType.UserNegotiated),
                Type = MessageType.SessionsStateChanged
            });
            _logger.LogInformation("UserNegotiated fired");
        }
        else
            _logger.LogDebug("Failed to broadcast about UserNegotiated. (Session not exist)");
    }
    
    public void UserLeavesVideoSession(string peerId, string videoSessionId)
    {
        if (GetVideoSession(videoSessionId, out var session) == SessionRequestResult.NoError)
        {
            BroadcastMessage(new BaseMessage()
            {
                Data = new SessionStateChanged(session!.AsModel(), SessionStateChangedType.UserDisconnected),
                Type = MessageType.SessionsStateChanged
            });
            _logger.LogInformation("UserLeaves fired");
        }
        else
            _logger.LogDebug("Failed to broadcast about UserLeaves. (Session not exist)");
    }
    
    public void UserKickedFormVideoSession(string peerId, string videoSessionId)
    {
        if (GetVideoSession(videoSessionId, out var session) == SessionRequestResult.NoError)
        {
            session?.ChangePeerApproveState(peerId, VideoSessionApproveState.Kicked);
            
            BroadcastMessage(new BaseMessage()
            {
                Data = new SessionStateChanged(session!.AsModel(), SessionStateChangedType.UserKicked),
                Type = MessageType.SessionsStateChanged
            });
            _logger.LogInformation("UserKicked fired");
        }
        else
            _logger.LogDebug("Failed to broadcast about UserKicked. (Session not exist)");
    }
    
    public void UserBannedInVideoSession(string peerId, string videoSessionId)
    {
        if (GetVideoSession(videoSessionId, out var session) == SessionRequestResult.NoError)
        {
            session?.ChangePeerApproveState(peerId, VideoSessionApproveState.Banned);
            
            BroadcastMessage(new BaseMessage()
            {
                Data = new SessionStateChanged(session!.AsModel(), SessionStateChangedType.UserBanned),
                Type = MessageType.SessionsStateChanged
            });
            _logger.LogInformation("UserBanned fired");
        }
        else
            _logger.LogDebug("Failed to broadcast about UserBanned. (Session not exist)");
    }
}