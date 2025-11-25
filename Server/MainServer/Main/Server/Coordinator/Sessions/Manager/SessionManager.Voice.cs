using Shared.Enums;
using Shared.Models;

namespace Server.MainServer.Main.Server.Coordinator.Sessions.Manager;

public partial class SessionManager
{
    public CreateSessionResult CreateVoiceSession(SessionDTO sessionDTO, string userId)
    {
        throw new NotImplementedException();
    }
    
    public JoinSessionResult JoinVoiceSession(SessionDTO sessionDTO, string userId)
    {
        throw new NotImplementedException();
    }
    
    public DeleteSessionResult DeleteVoiceSession(SessionDTO sessionDTO, string userId)
    {
        throw new NotImplementedException();
    }
    
    public LeaveSessionResult KickFromVoiceSession(SessionDTO sessionDTO, string userId)
    {
        throw new NotImplementedException();
    }
}