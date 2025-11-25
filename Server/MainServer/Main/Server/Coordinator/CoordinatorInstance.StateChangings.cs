using Shared.Enums;
using Shared.Models;
using Shared.Models.Requests.SessionInfo;

namespace Server.MainServer.Main.Server.Coordinator;

/// <summary>
/// Part of CoordinatorInstance class responsible for notifying users about any changes in the coordinator itself or its sessions
/// </summary>
public partial class CoordinatorInstance
{
    /// <summary>
    /// Mention all users in coordinator instance about new session
    /// </summary>
    /// <param name="sessionId"></param>
    public void SessionCreated(string sessionId)
    {
        if (GetVideoSession(sessionId, out var session) == SessionRequestResult.NoError)
        {
            VideoSessionAdded?.Invoke(session!.AsModel());
            
            BroadcastMessage(new BaseMessage()
            {
                Data = new SessionStateChanged(session!.AsModel(), SessionStateChangedType.SessionCreated),
                Type = MessageType.SessionsStateChanged
            });
            _logger.LogTrace("SessionCreated fired");
        }
        else
            _logger.LogDebug("Failed to broadcast about new session. (Session not exist)");
    }


    /// <summary>
    /// Mention all users in coordinator instance about deleted session
    /// </summary>
    /// <param name="sessionId"></param>
    public void SessionDeleted(string sessionId)
    {
        if (GetVideoSession(sessionId, out var session) == SessionRequestResult.NoError)
        {
            VideoSessionRemoved?.Invoke(sessionId);
            
            BroadcastMessage(new BaseMessage()
            {
                Data = new SessionStateChanged(session!.AsModel(), SessionStateChangedType.SessionDeleted),
                Type = MessageType.SessionsStateChanged
            });
            _logger.LogTrace("SessionDeleted fired");
        }
        else
            _logger.LogDebug("Failed to broadcast about session delete. (Session already deleted or not exist)");
    }
    
    /// <summary>
    /// Mention all users in coordinator instance about changes in session configuration
    /// </summary>
    /// <param name="sessionId"></param>
    public void SessionReconfigured(string sessionId)
    {
        if (GetVideoSession(sessionId, out var session) == SessionRequestResult.NoError)
        {
            VideoSessionReconfigured?.Invoke(session!.AsModel());
            
            BroadcastMessage(new BaseMessage()
            {
                Data = new SessionStateChanged(session!.AsModel(), SessionStateChangedType.SessionReconfigured),
                Type = MessageType.SessionsStateChanged
            });
            _logger.LogTrace("SessionReconfigured fired");
        }
        else
            _logger.LogDebug("Failed to broadcast about session reconfiguration. (Session not exist)");
    }
    
    /// <summary>
    /// Mention all users in session about host kick
    /// </summary>
    /// <param name="peerId"></param>
    /// <param name="videoSessionId"></param>
    public void HostJoinedVideoSession(string peerId, string videoSessionId)
    {
        if (GetVideoSession(videoSessionId, out var session) == SessionRequestResult.NoError)
        {
            BroadcastMessage(new BaseMessage()
            {
                Data = new SessionStateChanged(session!.AsModel(), SessionStateChangedType.HostConnected),
                Type = MessageType.SessionsStateChanged
            }, session.GetSessionId());
            _logger.LogTrace("HostJoined fired");
        }
        else
            _logger.LogDebug("Failed to broadcast about HostJoined. (Session not exist)");
    }
    
    /// <summary>
    /// Mention all users in session about joined host
    /// </summary>
    /// <param name="peerId"></param>
    /// <param name="videoSessionId"></param>
    public void HostLeavesVideoSession(string peerId, string videoSessionId)
    {
        if (GetVideoSession(videoSessionId, out var session) == SessionRequestResult.NoError)
        {
            BroadcastMessage(new BaseMessage()
            {
                Data = new SessionStateChanged(session!.AsModel(), SessionStateChangedType.HostDisconnected),
                Type = MessageType.SessionsStateChanged
            }, session.GetSessionId());
            _logger.LogTrace("HostLeaved fired");
        }
        else
            _logger.LogDebug("Failed to broadcast about HostLeaved. (Session not exist)");
    }
    
    /// <summary>
    /// Mention all users in session about host kick
    /// </summary>
    /// <param name="peerId"></param>
    /// <param name="videoSessionId"></param>
    public void HostKickedFromVideoSession(string peerId, string videoSessionId)
    {
        if (GetVideoSession(videoSessionId, out var session) == SessionRequestResult.NoError)
        {
            BroadcastMessage(new BaseMessage()
            {
                Data = new SessionStateChanged(session!.AsModel(), SessionStateChangedType.HostKicked),
                Type = MessageType.SessionsStateChanged
            }, session.GetSessionId());
            _logger.LogTrace("HostKicked fired");
        }
        else
            _logger.LogDebug("Failed to broadcast about HostKicked. (Session not exist)");
    }
    
    /// <summary>
    /// Mention all users in session about successful host WebRTC negotiation
    /// </summary>
    /// <param name="peerId"></param>
    /// <param name="videoSessionId"></param>
    public void HostNegotiatedInVideoSession(string peerId, string videoSessionId)
    {
        if (GetVideoSession(videoSessionId, out var session) == SessionRequestResult.NoError)
        {
            BroadcastMessage(new BaseMessage()
            {
                Data = new SessionStateChanged(session!.AsModel(), SessionStateChangedType.HostNegotiated),
                Type = MessageType.SessionsStateChanged
            }, session.GetSessionId());
            _logger.LogTrace("HostNegotiated fired");
        }
        else
            _logger.LogDebug("Failed to broadcast about HostNegotiated. (Session not exist)");
    }
    
    /// <summary>
    /// Mention all users in session about joined user
    /// </summary>
    /// <param name="peerId"></param>
    /// <param name="videoSessionId"></param>
    public void UserJoinedVideoSession(string peerId, string videoSessionId)
    {
        if (GetVideoSession(videoSessionId, out var session) == SessionRequestResult.NoError)
        {
            BroadcastMessage(new BaseMessage()
            {
                Data = new SessionStateChanged(session!.AsModel(), SessionStateChangedType.UserConnected),
                Type = MessageType.SessionsStateChanged
            }, session.GetSessionId());
            _logger.LogTrace("UserJoined fired");
        }
        else
            _logger.LogDebug("Failed to broadcast about UserJoined. (Session not exist)");
    }
    
    /// <summary>
    /// Mention all users in session about approved user
    /// </summary>
    /// <param name="peerId"></param>
    /// <param name="videoSessionId"></param>
    public void UserApprovedInVideoSession(string peerId, string videoSessionId)
    {
        if (GetVideoSession(videoSessionId, out var session) == SessionRequestResult.NoError)
        {
            BroadcastMessage(new BaseMessage()
            {
                Data = new SessionStateChanged(session!.AsModel(), SessionStateChangedType.UserApproved),
                Type = MessageType.SessionsStateChanged
            }, session.GetSessionId());
            _logger.LogTrace("UserApproved fired");
        }
        else
            _logger.LogDebug("Failed to broadcast about UserApproved. (Session not exist)");
    }
    
    /// <summary>
    /// Mention all users in session about rejected user
    /// </summary>
    /// <param name="peerId"></param>
    /// <param name="videSessionId"></param>
    public void UserRejectedInVideoSession(string peerId, string videSessionId)
    {
        if (GetVideoSession(videSessionId, out var session) == SessionRequestResult.NoError)
        {
            BroadcastMessage(new BaseMessage()
            {
                Data = new SessionStateChanged(session!.AsModel(), SessionStateChangedType.UserRejected),
                Type = MessageType.SessionsStateChanged
            }, session.GetSessionId());
            _logger.LogTrace("UserRejected fired");
        }
        else
            _logger.LogDebug("Failed to broadcast about UserRejected. (Session not exist)");
    }
    
    /// <summary>
    /// Mention all users in session about successful user WebRTC negotiation
    /// </summary>
    /// <param name="peerId"></param>
    /// <param name="videoSessionId"></param>
    public void UserNegotiatedInVideoSession(string peerId, string videoSessionId)
    {
        if (GetVideoSession(videoSessionId, out var session) == SessionRequestResult.NoError)
        {
            BroadcastMessage(new BaseMessage()
            {
                Data = new SessionStateChanged(session!.AsModel(), SessionStateChangedType.UserNegotiated),
                Type = MessageType.SessionsStateChanged
            }, session.GetSessionId());
            _logger.LogTrace("UserNegotiated fired");
        }
        else
            _logger.LogDebug("Failed to broadcast about UserNegotiated. (Session not exist)");
    }
    
    /// <summary>
    /// Mention all users in session about user leave
    /// </summary>
    /// <param name="peerId"></param>
    /// <param name="videoSessionId"></param>
    public void UserLeavesVideoSession(string peerId, string videoSessionId)
    {
        if (GetVideoSession(videoSessionId, out var session) == SessionRequestResult.NoError)
        {
            BroadcastMessage(new BaseMessage()
            {
                Data = new SessionStateChanged(session!.AsModel(), SessionStateChangedType.UserDisconnected),
                Type = MessageType.SessionsStateChanged
            }, session.GetSessionId());
            _logger.LogTrace("UserLeaves fired");
        }
        else
            _logger.LogDebug("Failed to broadcast about UserLeaves. (Session not exist)");
    }
    
    /// <summary>
    /// Mention all users in session about user kick
    /// </summary>
    /// <param name="peerId"></param>
    /// <param name="videoSessionId"></param>
    public void UserKickedFromVideoSession(string peerId, string videoSessionId)
    {
        if (GetVideoSession(videoSessionId, out var session) == SessionRequestResult.NoError)
        {
            BroadcastMessage(new BaseMessage()
            {
                Data = new SessionStateChanged(session!.AsModel(), SessionStateChangedType.UserKicked),
                Type = MessageType.SessionsStateChanged
            }, session.GetSessionId());
            _logger.LogTrace("UserKicked fired");
        }
        else
            _logger.LogDebug("Failed to broadcast about UserKicked. (Session not exist)");
    }
    
    /// <summary>
    /// Mention all users in session about user ban
    /// </summary>
    /// <param name="peerId"></param>
    /// <param name="videoSessionId"></param>
    public void UserBannedInVideoSession(string peerId, string videoSessionId)
    {
        if (GetVideoSession(videoSessionId, out var session) == SessionRequestResult.NoError)
        {
            BroadcastMessage(new BaseMessage()
            {
                Data = new SessionStateChanged(session!.AsModel(), SessionStateChangedType.UserBanned),
                Type = MessageType.SessionsStateChanged
            }, session.GetSessionId());
            _logger.LogTrace("UserBanned fired");
        }
        else
            _logger.LogDebug("Failed to broadcast about UserBanned. (Session not exist)");
    }
}