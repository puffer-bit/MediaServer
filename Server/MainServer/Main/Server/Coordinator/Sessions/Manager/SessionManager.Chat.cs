using Shared.Enums;
using Shared.Models;

namespace Server.MainServer.Main.Server.Coordinator.Sessions.Manager;

public partial class SessionManager
{
    public CreateSessionResult CreateChatSession(SessionDTO sessionDTO, string userId)
    {
        throw new NotImplementedException();
    }

    public JoinSessionResult JoinChatSession(SessionDTO sessionDTO, string userId)
    {
        throw new NotImplementedException();
    }

    public DeleteSessionResult DeleteChatSession(SessionDTO sessionDTO, string userId)
    {
        throw new NotImplementedException();
    }

    public LeaveSessionResult KickFromChatSession(SessionDTO sessionDTO, string userId)
    {
        throw new NotImplementedException();
    }
    
    /*internal LeaveSessionResult LeaveRoom(string roomId, string peerId)
    {
        try
        {
            if (VideoSessions.TryGetValue(roomId, out var session))
            {
                if (session.RemovePeer(peerId))
                {
                    logger.LogWarning("LeaveRoom event error. Requested peer not exist.");
                    return LeaveSessionResult.InternalError;
                }
                return LeaveSessionResult.NoError;
            }

            logger.LogWarning("LeaveRoom event error. Requested room not exist.");
            return LeaveSessionResult.InternalError;
        }
        catch (Exception e)
        {
            logger.LogWarning("LeaveRoom event error. Exception: {e}.", e.Message);
            return LeaveSessionResult.InternalError;
        }
    }*/
    
    public void RemoveUserFromAllSessions(string userId)
    {
        foreach (var session in _context.VideoSessions.Values)
        {
            var peersToRemove = session.GetAllPeers()
                .Where(p => p.GetUserId() == userId)
                .Select(p => p.GetId())
                .ToList();

            foreach (var peerId in peersToRemove)
            {
                KickFromVideoSession(session.AsModel(), peerId, false);
            }
        }
    }
}