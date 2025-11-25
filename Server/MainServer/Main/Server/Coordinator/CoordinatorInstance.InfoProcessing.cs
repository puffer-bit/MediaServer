using Server.MainServer.Main.Server.Coordinator.Sessions.Video;
using Shared.Enums;
using Shared.Models;

namespace Server.MainServer.Main.Server.Coordinator;

public partial class CoordinatorInstance
{
    internal IDictionary<string ,SessionDTO> GetAllSessions()
    {
        Dictionary<string, SessionDTO> rooms = new();
        foreach (var room in _sessionManager.GetAllVideoSessions())
        {
            rooms.Add(room.Key, room.Value.AsModel());
        }
        // foreach (var room in _sessionManager.GetAllVoiceSessions())
        // {
        //     rooms.Add(room.Value.AsModel());
        // }
        // foreach (var room in _sessionManager.GetAllChatSessions()())
        // {
        //     rooms.Add(room.Value.AsModel());
        // }
        return rooms;
    }
        
    internal SessionRequestResult GetVideoSessionAsModel(string roomId, out VideoSessionDTO? foundedSession)
    {
        try
        {
            if (_sessionManager.GetVideoSession(roomId, out var session))
            {
                foundedSession = session.AsModel();
                return SessionRequestResult.NoError;
            }
            else
            {
                _logger.LogDebug($"Error in GetSessionInfo. Requested room ({roomId}) not exists.");
                foundedSession = null;
                return SessionRequestResult.RoomNotExists;
            }
        }
        catch (Exception e)
        {
            _logger.LogWarning("GetSessionInfo event error. Exception: {e}.", e.Message);
            foundedSession = null;
            return SessionRequestResult.InternalError;
        }
    }
        
    internal SessionRequestResult GetVideoSession(string roomId, out IVideoSession? foundedSession)
    {
        try
        {
            if (_sessionManager.GetVideoSession(roomId, out var session))
            {
                foundedSession = session;
                return SessionRequestResult.NoError;
            }
            _logger.LogTrace($"Error in GetSessionInfo. Requested room ({roomId}) not exists.");
            foundedSession = null;
            return SessionRequestResult.RoomNotExists;
            
        }
        catch (Exception e)
        {
            _logger.LogWarning("GetSessionInfo event error. Exception: {e}.", e.Message);
            foundedSession = null;
            return SessionRequestResult.InternalError;
        }
    }
}