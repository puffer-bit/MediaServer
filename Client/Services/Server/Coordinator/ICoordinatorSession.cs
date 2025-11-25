using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using Client.Services.Server.Video;
using Shared.Enums;
using Shared.Models;
using Shared.Models.DTO;
using Shared.Models.Requests;
using SIPSorcery.Net;

namespace Client.Services.Server.Coordinator;

public interface ICoordinatorSession : IDisposable
{
    CoordinatorState ConnectionStatus { get; set; }
    DateTime? LastPing { get; }
    CancellationTokenSource Cts { get; set; }
    CoordinatorSessionDTO? CoordinatorDTO { get; set; }

    public event Action? OnDisconnect;
    public event NotifyCollectionChangedEventHandler? SessionsCollectionChanged;
    
    UserDTO GetUser();

    Task ConnectAndAuthenticate(UserDTO user, string address);
    Task ConnectAndAuthenticate(UserDTO user, string address, CancellationToken cancellationToken);
    Task Reconnect();
    Task Disconnect();
    void SetCoordinatorInstanceData(string coordinatorInstanceId);
    ObservableCollection<SessionDTO> GetAllSessions();
    Task<SessionDTO?> GetSessionById(string sessionId);
    Task UpdateAllSessionsAsync();

    void RaiseSessionAdded(SessionDTO sessionDTO);
    void RaiseSessionRemoved(SessionDTO sessionDTO);
    void RaiseSessionReconfigured(SessionDTO sessionDTO);
    void RaiseHostConnectedToSession(SessionDTO sessionDTO);
    void RaiseHostDisconnectedFromSession(SessionDTO sessionDTO);

    Task<CreateSessionResult> CreateSession(SessionDTO session);
    Task<DeleteSessionResult> DeleteSession(SessionDTO sessionDTO);
    Task<JoinSessionResult> JoinSession(SessionDTO session);
    void LeaveSession(SessionDTO session);
    bool ValidateSession(SessionDTO sessionDTO);
    
    IVideoSession? GetVideoSessionById(string sessionId);
    Task<RTCSessionDescriptionInit?> RequestOffer(WebRTCNegotiation request);
    void SendICE(WebRTCNegotiation request);
    void SendAnswer(WebRTCNegotiation answer);
    Task<UserDTO?> GetRemoteUser(string userId);
}