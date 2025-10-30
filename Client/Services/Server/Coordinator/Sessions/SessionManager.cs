using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Client.Services.Server.Coordinator.Authentication;
using Client.Services.Server.Coordinator.Connection;
using Client.Services.Server.Coordinator.Messaging;
using Client.Services.Server.Coordinator.UserManager;
using Client.Services.Server.Factories.VideoSessionFactory;
using Client.Services.Server.Video;
using DynamicData.Binding;
using Shared.Enums;
using Shared.Models;
using Shared.Models.Requests;
using Shared.Models.Requests.SessionInfo;
using Shared.Models.Requests.SessionActionsRequests;
using SIPSorcery.Net;

namespace Client.Services.Server.Coordinator.Sessions;

public class SessionManager : ISessionManager
{
    private readonly CoordinatorSession _coordinatorSession;
    private readonly ResponseAwaiter _awaiter;
    private readonly IVideoSessionFactory _videoSessionFactory;
    public ObservableCollection<SessionDTO> Sessions { get; } = new();
    public ConcurrentDictionary<string, IVideoSession> ActiveVideoSessions { get; } = new();
    public ConcurrentDictionary<string, IVideoSession> ActiveVoiceSessions { get; } = new();
    
    public SessionManager(
        ResponseAwaiter awaiter, 
        CoordinatorSession coordinatorSession, 
        IVideoSessionFactory videoSessionFactory)
    {
        _awaiter = awaiter;
        _coordinatorSession = coordinatorSession;
        _videoSessionFactory = videoSessionFactory;
        _videoSessionFactory.Initialize(_coordinatorSession);
    }

    public void HandleSessionStateChanged(SessionStateChanged stateChangedMessage)
    {
        switch (stateChangedMessage.Type)
        {
            case SessionStateChangedType.SessionCreated:
                AddSession(stateChangedMessage.Session);
                break;
            
            case SessionStateChangedType.SessionDeleted:
                RemoveSession(stateChangedMessage.Session.Id);
                break;
            
            case SessionStateChangedType.SessionReconfigured:
                ReconfigureSession(stateChangedMessage.Session);
                break;
            
            case SessionStateChangedType.HostConnected:
            case SessionStateChangedType.HostDisconnected:
                _coordinatorSession.RaiseHostConnectedToSession(stateChangedMessage.Session);
                if (ActiveVideoSessions.TryGetValue(stateChangedMessage.Session.Id, out var activeSession))
                {
                    if (activeSession.IsHost)
                        break;
                    if (stateChangedMessage.Type == SessionStateChangedType.HostConnected)
                    {
                        activeSession.HandleHostConnected();
                    }
                    else
                    {
                        activeSession.HandleHostDissconnected();
                    }
                }
                break;
            
            default:
                Console.WriteLine("HandleSessionStateChanged error. Received unknown type.");
                break;
        }
    }
    
    public async Task<SessionRequestResult?> RequestAllSessions()
    {
        Sessions.Clear();
        var message = new BaseMessage(_coordinatorSession.GetUser().Id!, MessageType.SessionInfoRequest, new SessionsInfoRequestModel(true));
        _coordinatorSession.SendMessage(message);

        var response = await _awaiter.WaitForResponseAsync(message.MessageId, message.Type);
        var sessionsModel = (SessionsInfoRequestModel)response.Data;
        if (sessionsModel.Result == SessionRequestResult.NoError)
        {
            foreach (var session in sessionsModel.SessionsList)
            {
                Sessions.Add(session.Value);
            }
        }

        return sessionsModel.Result;
    }

    public async Task<SessionRequestResult?> RequestSessionById(string sessionId)
    {
        var message = new BaseMessage(_coordinatorSession.GetUser().Id!, MessageType.SessionInfoRequest, new SessionsInfoRequestModel(false)
        {
            RoomId = sessionId
        });
        _coordinatorSession.SendMessage(message);

        var response = await _awaiter.WaitForResponseAsync(message.MessageId, message.Type);
        var sessionsModel = (SessionsInfoRequestModel)response.Data;
        if (sessionsModel.Result == SessionRequestResult.NoError)
        {
            for (int i = 0; i < Sessions.Count; i++)
            {
                if (Sessions[i].Id == sessionsModel.SessionsList.FirstOrDefault().Key)
                {
                    Sessions[i] = sessionsModel.SessionsList.FirstOrDefault().Value;
                }
            }
        }

        return sessionsModel.Result;
    }

    public SessionDTO? GetSessionById(string sessionId)
    {
        for (int i = 0; i <= Sessions.Count; i++)
        {
            if (Sessions[i].Id == sessionId)
            {
                return Sessions[i];
            }
        }
        return null;
    }

    public IVideoSession? GetVideoSessionById(string sessionId)
    {
        foreach (var session in ActiveVideoSessions)
        {
            return session.Value;
        }
        return null;
    }

    public void ReactOnICE(WebRTCNegotiation request)
    {
        if (ActiveVideoSessions.TryGetValue(request.RoomId!, out var videoSession))
        {
            if (videoSession.State == VideoSessionState.ReadyForNegotiation || videoSession.State == VideoSessionState.Connected)
            {
                videoSession.Peer.PeerConnection!.addIceCandidate(request.Data as RTCIceCandidateInit);
            }
            else
            {
                videoSession.IceCandidatesBuffer.Enqueue(request.Data as RTCIceCandidateInit);
            }
        }
    }

    public void AddSession(SessionDTO sessionDTO)
    {
        for (int i = 0; i < Sessions.Count; i++)
        {
            if (Sessions[i].Id == sessionDTO.Id)
            {
                ReconfigureSession(sessionDTO);
                _coordinatorSession.RaiseSessionReconfigured(sessionDTO);
                Console.WriteLine("Attempted create existing session. Reconfigured instead.");
                return;
            }
        }
        Sessions.Add(sessionDTO);
        _coordinatorSession.RaiseSessionAdded(sessionDTO);
    }
    
    public void RemoveSession(string sessionId)
    {
        for (int i = 0; i < Sessions.Count; i++)
        {
            if (Sessions[i].Id == sessionId)
            {
                _coordinatorSession.RaiseSessionRemoved(Sessions[i]);
                Sessions.Remove(Sessions[i]);
                return;
            }
        }
        Console.WriteLine("Attempted delete not existing session");
    }

    public void ReconfigureSession(SessionDTO sessionDTO)
    {
        for (int i = 0; i < Sessions.Count; i++)
        {
            if (Sessions[i].Id == sessionDTO.Id)
            {
                Sessions[i] = sessionDTO;
                _coordinatorSession.RaiseSessionReconfigured(sessionDTO);
                return;
            }
        }
        Console.WriteLine("Attempted reconfigure not existing session");
    }

    public async Task<CreateSessionResult> CreateSession(SessionDTO session)
    {
        var message = new BaseMessage(_coordinatorSession.GetUser().Id!, MessageType.RoomRequest, new CreateSessionRequestModel(session));
        _coordinatorSession.SendMessage(message);

        var response = await _awaiter.WaitForResponseAsync(message.MessageId, message.Type);
        var responseData = response.Data as CreateSessionRequestModel;

        if (responseData == null || responseData!.Result == null)
            return CreateSessionResult.InternalError;
        return (CreateSessionResult)responseData.Result!;
    }
    
    public async Task<DeleteSessionResult> DeleteSession(SessionDTO sessionDTO)
    {
        var message = new BaseMessage(_coordinatorSession.GetUser().Id!, MessageType.RoomRequest, new DeleteSessionRequestModel(sessionDTO));
        _coordinatorSession.SendMessage(message);

        var response = await _awaiter.WaitForResponseAsync(message.MessageId, message.Type);
        var responseData = response.Data as DeleteSessionRequestModel;

        if (responseData == null || responseData!.Result == null)
            return DeleteSessionResult.InternalError;
        return (DeleteSessionResult)responseData.Result!;
    }
    
    public async Task<JoinSessionResult> JoinSession(SessionDTO session)
    {
        var message = new BaseMessage(_coordinatorSession.GetUser().Id!, MessageType.RoomRequest, new JoinSessionRequestModel(session));
        _coordinatorSession.SendMessage(message);

        var response = await _awaiter.WaitForResponseAsync(message.MessageId, message.Type);
        var responseData = (JoinSessionRequestModel)response.Data;
        if (responseData.Result == JoinSessionResult.NoError)
        {
            if (responseData.Session.SessionType == SessionType.Video)
            {
                ActiveVideoSessions[responseData.Session.Id] = _videoSessionFactory.CreateVideoSession(
                    (VideoSessionDTO)responseData.Session,
                    responseData.PeerId!);
            }
        }

        return (JoinSessionResult)responseData.Result!;
    }
    
    public void LeaveSession(SessionDTO session)
    {
        var message = new BaseMessage(_coordinatorSession.GetUser().Id!, MessageType.RoomRequest, new LeaveSessionRequestModel(session));
        _coordinatorSession.SendMessage(message);
        ActiveVideoSessions.Remove(session.Id, out _);
    }
}