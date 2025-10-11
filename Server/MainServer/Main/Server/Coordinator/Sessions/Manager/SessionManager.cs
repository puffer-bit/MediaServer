using Server.MainServer.Main.Server.Factories.VideoSessionFactory;
using Server.MainServer.Main.Server.Video;
using Shared.Enums;
using Shared.Models;
using Shared.Models.Requests.SessionInfo;

namespace Server.MainServer.Main.Server.Coordinator.Manager;

public class SessionManager : ISessionManager
{
    private readonly ILogger _logger;
    private readonly IVideoSessionFactory _videoSessionFactory;
    private readonly ISessionManagerContext _context;
    private readonly CoordinatorInstance _coordinator;

    public SessionManager(ILoggerFactory loggerFactory, 
        ISessionManagerContext context,
        IVideoSessionFactory videoSessionFactory, 
        CoordinatorInstance coordinator)
    {
        _logger = loggerFactory.CreateLogger("SessionManager");       
        _context = context;
        _videoSessionFactory = videoSessionFactory;
        _coordinator = coordinator;
    }
    
    public CreateSessionResult CreateChatSession(SessionDTO sessionDTO, string userId)
    {
        throw new NotImplementedException();
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
                _logger.LogInformation("CreateVideoSession event error. Wrong room capacity");
                return CreateSessionResult.WrongCapacity;
            }
        
            var newRoomId = sessionDTO.HostId == "system" ? sessionDTO.Id : Guid.NewGuid().ToString();
            videoSessionDTO.Id = newRoomId;
            if (_context.VideoSessions.TryAdd(newRoomId, _videoSessionFactory.CreateVideoSession(videoSessionDTO, _coordinator)))
            {
                _logger.LogInformation("Room with name \"{Name}\"(ID: {newRoomId}) created. Waiting for host...", sessionDTO.Name, sessionDTO.Id);
                _coordinator.SessionCreated(sessionDTO.Id);
                return CreateSessionResult.NoError;
            }
            _logger.LogError("CreateVideoSession event error. Room with name \"{Name}\"(ID: {newRoomId}) already exists", sessionDTO.Name, sessionDTO.Id);
            return CreateSessionResult.NameAlreadyUsed;
        }
        catch (Exception e)
        {
            _logger.LogWarning("CreateVideoSession event error. Exception: {e}", e.Message);
            return CreateSessionResult.InternalError;
        }
    }

    public IVideoSession? GetVideoSession(string roomId)
    {
        throw new NotImplementedException();
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

    public void SetHostId(string roomId, string hostId)
    {
        throw new NotImplementedException();
    }

    public bool GetHostId(string roomId, out string? hostId)
    {
        if (_context.VideoSessions.TryGetValue(roomId, out var session))
        {
            hostId = session.GetHostId();
            return true;
        }

        hostId = null;
        return false;
    }

    public CreateSessionResult CreateVoiceSession(SessionDTO sessionDTO, string userId)
    {
        throw new NotImplementedException();
    }

    public JoinSessionResult JoinChatSession(SessionDTO sessionDTO, string userId)
    {
        throw new NotImplementedException();
    }

    public JoinSessionResult JoinVideoSession(SessionDTO sessionDTO, string userId)
    {
        try
        {
            if (_context.VideoSessions.TryGetValue(sessionDTO.Id!, out var session))
            {
                if (session.GetAllPeers().Count >= session.GetSessionCapacity())
                {
                    _logger.LogWarning("JoinRoom event error. Room full.");
                    return JoinSessionResult.RoomFull;
                }
                
                session.CreateAndAttachPeer(userId);
                
                return JoinSessionResult.NoError;
            }

            _logger.LogWarning("JoinRoom event error. Room not exist.");
            return JoinSessionResult.RoomNotExists;
        }
        catch (Exception e)
        {
            _logger.LogWarning("JoinRoom event error. Exception: {e}.", e.Message);
            return JoinSessionResult.InternalError;
        }
    }

    public JoinSessionResult JoinVoiceSession(SessionDTO sessionDTO, string userId)
    {
        throw new NotImplementedException();
    }

    public DeleteSessionResult DeleteChatSession(SessionDTO sessionDTO, string userId)
    {
        throw new NotImplementedException();
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
            if (_context.VideoSessions.TryRemove(sessionDTO.Id!, out var session))
            {
                _logger.LogInformation("Room with name \"{Name}\"(ID: {newRoomId}) deleted.", sessionDTO.Name, sessionDTO.Id);
                _coordinator.SessionDeleted(sessionDTO);
                return DeleteSessionResult.NoError;
            }
            
            _logger.LogWarning("DeleteRoom event error. Room not exists");
            return DeleteSessionResult.RoomNotExists;
        }
        catch (Exception e)
        {
            _logger.LogWarning("DeleteRoom event error. Exception: {e}.", e.Message);
            return DeleteSessionResult.InternalError;
        }
    }

    public DeleteSessionResult DeleteVoiceSession(SessionDTO sessionDTO, string userId)
    {
        throw new NotImplementedException();
    }

    public LeaveSessionResult KickFromChatSession(SessionDTO sessionDTO, string userId)
    {
        throw new NotImplementedException();
    }

    public LeaveSessionResult KickFromVideoSession(SessionDTO sessionDTO, string userId)
    {
        try
        {
            if (_context.VideoSessions.TryGetValue(sessionDTO.Id!, out var session))
            {
                if (session.GetPeerByUserId(userId, out var peer))
                {
                    session.DetachPeer(peer!.GetId());
                }
                
                return LeaveSessionResult.NoError;
            }

            _logger.LogWarning("LeaveSession event error. Room not exist.");
            return LeaveSessionResult.RoomNotExists;
        }
        catch (Exception e)
        {
            _logger.LogWarning("LeaveSession event error. Exception: {e}", e.Message);
            return LeaveSessionResult.InternalError;
        }
    }

    public LeaveSessionResult KickFromVoiceSession(SessionDTO sessionDTO, string userId)
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
                KickFromVideoSession(session.AsModel(), peerId);
            }
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
                _logger.LogWarning("SetHostId event error. Room not exist.");
            }
        }
        catch (Exception e)
        {
            _logger.LogWarning("SetHostId event error. Exception: {e}.", e.Message);
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
                _logger.LogWarning("SetHostId event error. Room not exist.");
            }
        }
        catch (Exception e)
        {
            _logger.LogWarning("SetHostId event error. Exception: {e}.", e.Message);
        }
    }
}