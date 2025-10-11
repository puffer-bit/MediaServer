using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Client.Services.Server.Video;
using Shared.Enums;
using Shared.Models;

namespace Client.Services.Server.Coordinator;

public partial class CoordinatorSession
{
    public async Task UpdateAllSessionsAsync()
    {
        try
        {
            await _sessionsManager.RequestAllSessions();
        }
        catch (TimeoutException)
        {
            ConnectionStatus = CoordinatorState.TimedOut;
        }
    }
    
    public ObservableCollection<SessionDTO> GetAllSessions()
    {
        return _sessionsManager.Sessions;
    }

    public async Task<SessionDTO?> GetSessionById(string sessionId)
    {
        if (await _sessionsManager.RequestSessionById(sessionId) != SessionRequestResult.NoError)
        {
            return null;
        }
        return _sessionsManager.GetSessionById(sessionId);
    }
    
    public IVideoSession? GetVideoSessionById(string sessionId)
    {
        return _sessionsManager.GetVideoSessionById(sessionId);
    }
    
    public async Task<CreateSessionResult> CreateSession(SessionDTO session)
    {
        try
        {
            if (!ValidateSession(session))
                return CreateSessionResult.UnexceptedParameters;
            
            return await _sessionsManager.CreateSession(session);
        }
        catch (TimeoutException)
        {
            ConnectionStatus = CoordinatorState.TimedOut;
            return CreateSessionResult.TimedOut;
        }
    }

    public async Task<DeleteSessionResult> DeleteSession(SessionDTO sessionDTO)
    {
        try
        {
            return await _sessionsManager.DeleteSession(sessionDTO);
        }
        catch (TimeoutException)
        {
            ConnectionStatus = CoordinatorState.TimedOut;
            return DeleteSessionResult.TimedOut;
        }
    }


    public async Task<JoinSessionResult> JoinSession(SessionDTO session)
    {
        try
        {
            var result = await _sessionsManager.JoinSession(session);

            return result;
        }
        catch (TimeoutException)
        {
            return JoinSessionResult.TimedOut;
        }
    }
    
    public void LeaveSession(SessionDTO session)
    {
        _sessionsManager.LeaveSession(session);
    }
    
    public bool ValidateSession(SessionDTO sessionDTO)
    {
        return true;
    }
}