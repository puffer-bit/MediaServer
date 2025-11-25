using Shared.Enums;
using Shared.Models;

namespace Server.MainServer.Main.Server.Coordinator
{
    public partial class CoordinatorInstance
    {
        // TODO: Implement voice and chat conditions
        /// <summary>
        ///  Creates room from model
        /// </summary>
        /// <param name="sessionDTO"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public CreateSessionResult CreateSession(SessionDTO sessionDTO, string userId)
        {
            switch (sessionDTO.SessionType)
            {
                case SessionType.Chat:
                    return _sessionManager.CreateChatSession(sessionDTO, userId);

                case SessionType.Video:
                    return _sessionManager.CreateVideoSession(sessionDTO, userId);

                case SessionType.Voice:
                    return _sessionManager.CreateVoiceSession(sessionDTO, userId);

                case SessionType.Undefiend:
                default:
                    return CreateSessionResult.InternalError;
            }
        }
        
        public DeleteSessionResult DeleteSession(SessionDTO sessionDTO, string userId)
        {
            switch (sessionDTO.SessionType)
            {
                case SessionType.Chat:
                    return _sessionManager.DeleteChatSession((ChatSessionDTO)sessionDTO, userId);

                case SessionType.Video:
                    return _sessionManager.DeleteVideoSession((VideoSessionDTO)sessionDTO, userId);

                case SessionType.Voice:
                    return _sessionManager.DeleteVoiceSession((VoiceSessionDTO)sessionDTO, userId);

                case SessionType.Undefiend:
                default:
                    return DeleteSessionResult.InternalError;
            }
        }
        
        public JoinSessionResult EnterSession(SessionDTO sessionDTO, string userId)
        {
            switch (sessionDTO.SessionType)
            {
                case SessionType.Chat:
                    return _sessionManager.JoinChatSession((ChatSessionDTO)sessionDTO, userId);

                case SessionType.Video:
                    return _sessionManager.JoinVideoSession((VideoSessionDTO)sessionDTO, userId);

                case SessionType.Voice:
                    return _sessionManager.JoinVoiceSession((VoiceSessionDTO)sessionDTO, userId);

                case SessionType.Undefiend:
                default:
                    return JoinSessionResult.InternalError;
            }
        }
        
        public LeaveSessionResult KickFromSession(SessionDTO sessionDTO, string userId, bool isForce)
        {
            switch (sessionDTO.SessionType)
            {
                case SessionType.Chat:
                    return _sessionManager.KickFromChatSession((ChatSessionDTO)sessionDTO, userId);

                case SessionType.Video:
                    return _sessionManager.KickFromVideoSession((VideoSessionDTO)sessionDTO, userId, isForce);

                case SessionType.Voice:
                    return _sessionManager.KickFromVoiceSession((VoiceSessionDTO)sessionDTO, userId);

                case SessionType.Undefiend:
                default:
                    return LeaveSessionResult.InternalError;
            }
        }
        
        public ApproveUserSessionResult? ApproveUserInSession(SessionDTO sessionDTO, string userId)
        {
            switch (sessionDTO.SessionType)
            {
                case SessionType.Chat:
                    throw new NotImplementedException();

                case SessionType.Video:
                    return _sessionManager.ApproveVideoSessionParticipant(sessionDTO, userId);

                case SessionType.Voice:
                    throw new NotImplementedException();

                case SessionType.Undefiend:
                default:
                    return ApproveUserSessionResult.InternalError;
            }
        }
        
        public RejectUserSessionResult? RejectUserInSession(SessionDTO sessionDTO, string userId)
        {
            switch (sessionDTO.SessionType)
            {
                case SessionType.Chat:
                    throw new NotImplementedException();

                case SessionType.Video:
                    return _sessionManager.RejectVideoSessionParticipant(sessionDTO, userId);

                case SessionType.Voice:
                    throw new NotImplementedException();

                case SessionType.Undefiend:
                default:
                    return RejectUserSessionResult.InternalError;
            }
        }
        
        public void RemoveUserFromAllSessions(string userId)
        {
            _sessionManager.RemoveUserFromAllSessions(userId);
        }

        public void SetVideoSessionHost(VideoSessionDTO videoSessionDTO, string? hostPeerId)
        {
            _sessionManager.SetVideoSessionHost(videoSessionDTO, hostPeerId);
        }

        public void RemoveVideoSessionHost(VideoSessionDTO videoSessionDTO)
        {
            _sessionManager.RemoveVideoSessionHost(videoSessionDTO);
        }
    }
}
