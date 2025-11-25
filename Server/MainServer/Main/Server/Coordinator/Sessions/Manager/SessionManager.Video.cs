using Server.Domain.Entities;
using Server.MainServer.Main.Server.Coordinator.Sessions.Video;
using Shared.Enums;
using Shared.Models;

namespace Server.MainServer.Main.Server.Coordinator.Sessions.Manager;

public partial class SessionManager
{
    public void FetchVideoSessionsFromList(List<VideoSessionEntity> sessionEntities)
    {
        foreach (VideoSessionEntity sessionEntity in sessionEntities)
        {
            _context.VideoSessions.TryAdd(sessionEntity.Id, _videoSessionFactory.CreateVideoSession(
                new VideoSessionDTO()
                {
                    CoordinatorInstanceId = sessionEntity.CoordinatorInstanceId,
                    Id = sessionEntity.Id,
                    Name = sessionEntity.Name,
                    Capacity = sessionEntity.Capacity,
                    HostId = sessionEntity.HostId,
                    SessionType = SessionType.Video,
                }, _coordinator));
        }
    }
    
    /// <summary>
    ///  Creates session from VideoSessionDTO
    /// </summary>
    /// <param name="sessionDTO"></param>
    /// <param name="userId"></param>
    public CreateSessionResult CreateVideoSession(SessionDTO sessionDTO, string userId)
    {
        try
        {
            var videoSessionDTO = (VideoSessionDTO)sessionDTO;
            if (sessionDTO.Capacity <= 1)
            {
                _logger.LogTrace("CreateVideoSession event error. Wrong room capacity");
                return CreateSessionResult.WrongCapacity;
            }
        
            var newRoomId = sessionDTO.HostId == "system" ? sessionDTO.Id : Guid.NewGuid().ToString();
            videoSessionDTO.Id = newRoomId;
            if (_context.VideoSessions.TryAdd(newRoomId, _videoSessionFactory.CreateVideoSession(videoSessionDTO, _coordinator)))
            {
                _logger.LogTrace("Room with name \"{Name}\"(ID: {newRoomId}) created. Waiting for host...", sessionDTO.Name, sessionDTO.Id);
                _coordinator.SessionCreated(sessionDTO.Id);
                return CreateSessionResult.NoError;
            }
            _logger.LogTrace("Failed to create video session. Room with name \"{Name}\"(ID: {newRoomId}) already exists and identical session names are prohibited.", sessionDTO.Name, sessionDTO.Id);
            return CreateSessionResult.NameAlreadyUsed;
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to create video session. Exception: {e}", e.Message);
            return CreateSessionResult.InternalError;
        }
    }
    
    public JoinSessionResult JoinVideoSession(SessionDTO sessionDTO, string userId)
    {
        try
        {
            if (_context.VideoSessions.TryGetValue(sessionDTO.Id!, out var session))
            {
                return session.AttachPeer(session.CreatePeer(userId));
            }

            _logger.LogTrace("User with ID {userId} failed to connect. Room not exists.", userId);
            return JoinSessionResult.RoomNotExists;
        }
        catch (Exception e)
        {
            _logger.LogError("JoinRoom event error. Exception: {e}.", e.Message);
            return JoinSessionResult.InternalError;
        }
    }

    public ApproveUserSessionResult ApproveVideoSessionParticipant(SessionDTO sessionDTO, string userId)
    {
        try
        {
            if (_context.VideoSessions.TryGetValue(sessionDTO.Id!, out var session))
            {
                return session.ApprovePeerByUserId(userId);
            }

            _logger.LogTrace("JoinRoom event error. Room not exist.");
            return ApproveUserSessionResult.RoomNotExists;
        }
        catch (Exception e)
        {
            _logger.LogError("JoinRoom event error. Exception: {e}.", e.Message);
            return ApproveUserSessionResult.InternalError;
        }
    }
    
    public RejectUserSessionResult RejectVideoSessionParticipant(SessionDTO sessionDTO, string userId)
    {
        try
        {
            if (_context.VideoSessions.TryGetValue(sessionDTO.Id!, out var session))
            {
                return session.RejectPeerByUserId(userId);
            }

            _logger.LogTrace("JoinRoom event error. Room not exist.");
            return RejectUserSessionResult.RoomNotExists;
        }
        catch (Exception e)
        {
            _logger.LogError("JoinRoom event error. Exception: {e}.", e.Message);
            return RejectUserSessionResult.InternalError;
        }
    }
    
    public LeaveSessionResult KickFromVideoSession(SessionDTO sessionDTO, string userId, bool isForce)
    {
        try
        {
            if (_context.VideoSessions.TryGetValue(sessionDTO.Id!, out var session))
            {
                if (session.GetPeerByUserId(userId, out var peer))
                {
                    session.DetachPeer(peer!.GetId(), isForce);
                }
                
                return LeaveSessionResult.NoError;
            }

            _logger.LogTrace("LeaveSession event error. Room not exist.");
            return LeaveSessionResult.RoomNotExists;
        }
        catch (Exception e)
        {
            _logger.LogError("LeaveSession event error. Exception: {e}", e.Message);
            return LeaveSessionResult.InternalError;
        }
    }
    
    /// <summary>
    ///  Delete video session by VideoSessionDTO
    /// </summary>
    /// <param name="sessionDTO"></param>
    /// <param name="userId"></param>
    public DeleteSessionResult DeleteVideoSession(SessionDTO sessionDTO, string userId)
    {
        try
        {
            _coordinator.SessionDeleted(sessionDTO.Id!);
            if (_context.VideoSessions.TryRemove(sessionDTO.Id!, out var session))
            {
                session.CloseSession();
                _logger.LogTrace("Room with name \"{Name}\"(ID: {newRoomId}) deleted.", sessionDTO.Name, sessionDTO.Id);
                return DeleteSessionResult.NoError;
            }
            
            _logger.LogTrace("DeleteRoom event error. Room not exists");
            return DeleteSessionResult.RoomNotExists;
        }
        catch (Exception e)
        {
            _logger.LogError("DeleteRoom event error. Exception: {e}.", e.Message);
            return DeleteSessionResult.InternalError;
        }
    }
    
    public void SetVideoSessionHost(SessionDTO videoSessionDTO, string? hostPeerId)
    {
        try
        {
            if (_context.VideoSessions.TryGetValue(videoSessionDTO.Id!, out var session))
            {
                session.SetHostPeerId(hostPeerId);
            }
            else
            {
                _logger.LogTrace("SetHostId event error. Room not exist.");
            }
        }
        catch (Exception e)
        {
            _logger.LogError("SetHostId event error. Exception: {e}.", e.Message);
        }
    }

    public void RemoveVideoSessionHost(SessionDTO videoSessionDTO)
    {
        try
        {
            if (_context.VideoSessions.TryGetValue(videoSessionDTO.Id!, out var session))
            {
                session.SetHostPeerId(null);
            }
            else
            {
                _logger.LogTrace("SetHostId event error. Room not exist.");
            }
        }
        catch (Exception e)
        {
            _logger.LogError("SetHostId event error. Exception: {e}.", e.Message);
        }
    }
    
    public bool GetVideoSession(string roomId, out IVideoSession? videoSession)
    {
        if (_context.VideoSessions.TryGetValue(roomId, out var session))
        {
            videoSession = session;
            return true;
        }
        videoSession = null;
        return false;
    }
    
    public IDictionary<string, IVideoSession> GetAllVideoSessions()
    {
        return _context.VideoSessions;
    }

    public IDictionary<string, SessionDTO> GetAllVideoSessionsAsModel()
    {
        Dictionary<string, SessionDTO> videoSessionDtos = new Dictionary<string, SessionDTO>();
        foreach (var videoSession in _context.VideoSessions)
        {
            videoSessionDtos.Add(videoSession.Key, videoSession.Value.AsModel());
        }

        return videoSessionDtos;
    }
    
    public bool GetVideoSessionHostId(string roomId, out string? hostId)
    {
        if (_context.VideoSessions.TryGetValue(roomId, out var session))
        {
            hostId = session.GetHostId();
            return true;
        }

        hostId = null;
        return false;
    }
}