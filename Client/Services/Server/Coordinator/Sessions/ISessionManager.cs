using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Client.Services.Server.Video;
using Shared.Enums;
using Shared.Models;
using Shared.Models.Requests;
using Shared.Models.Requests.SessionInfo;

namespace Client.Services.Server.Coordinator.Sessions;

public interface ISessionManager
{
    ObservableCollection<SessionDTO> Sessions { get; }
    
    void HandleSessionStateChanged(SessionStateChanged stateChangedMessage);
    
    Task<SessionRequestResult?> RequestAllSessions();
    Task<SessionRequestResult?> RequestSessionById(string sessionId);
    
    SessionDTO? GetSessionById(string sessionId);
    Task<CreateSessionResult> CreateSession(SessionDTO session);
    Task<JoinSessionResult> JoinSession(SessionDTO session);
    void LeaveSession(SessionDTO session);
    
    IVideoSession? GetVideoSessionById(string sessionId);
    void ReactOnICE(WebRTCNegotiation request);
    void RemoveSession(string sessionId);
    void AddSession(SessionDTO sessionDTO);
    void ReconfigureSession(SessionDTO sessionDTO);
    Task<DeleteSessionResult> DeleteSession(SessionDTO sessionDTO);
}