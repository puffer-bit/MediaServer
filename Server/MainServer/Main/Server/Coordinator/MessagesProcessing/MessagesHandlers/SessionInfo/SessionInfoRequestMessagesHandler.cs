using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared.Enums;
using Shared.Models;
using Shared.Models.Requests.SessionInfo;

namespace Server.MainServer.Main.Server.Coordinator.MessagesProcessing.MessagesHandlers.SessionInfo;

public class SessionInfoRequestMessagesHandler : IMessageHandler
{
    private readonly CoordinatorInstance _coordinator;
        
    public MessageType Type => MessageType.SessionInfoRequest;
        
    public SessionInfoRequestMessagesHandler(
        CoordinatorInstance coordinator)
    {
        _coordinator = coordinator;
    }

    public Task<HandleMessageResult> HandleMessage(BaseMessage message)
    {
        try
        {
            var infoRequest = (SessionsInfoRequestModel)message.Data;

            if (infoRequest != null)
            {
                if (infoRequest.IsAllSessionsRequested)
                { 
                    HandleSessionsInfo(infoRequest, message);
                }
                else if (!infoRequest.IsAllSessionsRequested && infoRequest.RoomId != null)
                {
                    HandleSessionInfo(infoRequest, message);
                }
                else
                {
                    SendResponse(infoRequest, SessionRequestResult.InternalError, message);
                }
                return Task.FromResult(HandleMessageResult.ForbiddenMessage);
            }
        }
        catch (JsonException ex)
        {
            SendResponse(new SessionsInfoRequestModel(false), SessionRequestResult.InternalError, message);
            return Task.FromResult(HandleMessageResult.JsonParseError);
        }

        return Task.FromResult(HandleMessageResult.NoError);
    }

    private void HandleSessionsInfo(SessionsInfoRequestModel infoRequest, BaseMessage message)
    {
        infoRequest.AddSessions(_coordinator.GetAllSessions());
        SendResponse(infoRequest, SessionRequestResult.NoError, message);
    }
        
    private void HandleSessionInfo(SessionsInfoRequestModel infoRequest, BaseMessage message)
    {
        if (_coordinator.GetVideoSessionAsModel(infoRequest.RoomId, out var session) == SessionRequestResult.NoError)
        {
            infoRequest.AddSession(session!.Id, session);
            SendResponse(infoRequest, SessionRequestResult.NoError, message);
        }
        SendResponse(infoRequest, SessionRequestResult.RoomNotExists, message);
    }
        
    private void SendResponse(SessionsInfoRequestModel request, SessionRequestResult result, BaseMessage message)
    {
        request.Result = result;
        _coordinator.SendMessageToUser(message.UserId!, new BaseMessage(message)
        {
            Data = request
        });
    }
}