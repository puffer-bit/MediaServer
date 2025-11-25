using Server.Domain.Entities;
using Server.MainServer.Main.Server.Coordinator.Sessions.Video;
using Shared.Enums;
using Shared.Models;

namespace Server.MainServer.Main.Server.Coordinator.Sessions.Manager;

public interface ISessionManager
{
    CreateSessionResult CreateChatSession(SessionDTO sessionDTO, string userId);
    JoinSessionResult JoinChatSession(SessionDTO sessionDTO, string userId);
    LeaveSessionResult KickFromChatSession(SessionDTO sessionDTO, string userId);
    DeleteSessionResult DeleteChatSession(SessionDTO sessionDTO, string userId);

    void FetchVideoSessionsFromList(List<VideoSessionEntity> sessionEntities);
    CreateSessionResult CreateVideoSession(SessionDTO sessionDTO, string userId);
    JoinSessionResult JoinVideoSession(SessionDTO sessionDTO, string userId);
    LeaveSessionResult KickFromVideoSession(SessionDTO sessionDTO, string userId, bool isForce);
    DeleteSessionResult DeleteVideoSession(SessionDTO sessionDTO, string userId);
    ApproveUserSessionResult ApproveVideoSessionParticipant(SessionDTO sessionDTO, string userId);
    RejectUserSessionResult RejectVideoSessionParticipant(SessionDTO sessionDTO, string userId);
    IDictionary<string, IVideoSession> GetAllVideoSessions();
    IDictionary<string, SessionDTO> GetAllVideoSessionsAsModel();
    bool GetVideoSession(string roomId, out IVideoSession? videoSession);
    bool GetVideoSessionHostId(string roomId, out string? hostId);
    
    CreateSessionResult CreateVoiceSession(SessionDTO sessionDTO, string userId);
    JoinSessionResult JoinVoiceSession(SessionDTO sessionDTO, string userId);
    LeaveSessionResult KickFromVoiceSession(SessionDTO sessionDTO, string userId);
    DeleteSessionResult DeleteVoiceSession(SessionDTO sessionDTO, string userId);
    
    void RemoveUserFromAllSessions(string userId);
    void SetVideoSessionHost(SessionDTO videoSessionDTO, string? hostPeerId);
    void RemoveVideoSessionHost(SessionDTO videoSessionDTO);
    
    void CloseAllSessions();
}