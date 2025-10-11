using Shared.Enums;
using Shared.Models.Requests.SessionActionsRequests;

namespace Server.MainServer.Main.Server.Coordinator.MessagesProcessing.MessagesHandlers.SessionActions;

public class SessionResponseFactory
{
    public object CreateResponse(IUserSessionRequestModel request, SessionRequestResult result)
    {
        switch (request.Type)
        {
            case SessionRequestType.Create:
                var response = (CreateSessionRequestModel)request;
                response.Result = result;
                break;
            
            case SessionRequestType.Join:
                break;
            
            case SessionRequestType.Leave:
                break;
            
            case SessionRequestType.Delete:
                break;
        }
    }
}