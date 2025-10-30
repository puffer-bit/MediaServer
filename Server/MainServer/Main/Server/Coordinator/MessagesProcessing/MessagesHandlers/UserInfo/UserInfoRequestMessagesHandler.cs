using Newtonsoft.Json;
using Shared.Enums;
using Shared.Models;
using Shared.Models.Requests.SessionInfo;
using Shared.Models.Requests.Users;

namespace Server.MainServer.Main.Server.Coordinator.MessagesProcessing.MessagesHandlers.UserInfo;

public class UserInfoRequestMessagesHandler : IMessageHandler
{
    private readonly CoordinatorInstance _coordinator;
        
    public MessageType Type => MessageType.UserInfoRequest;
        
    public UserInfoRequestMessagesHandler(
        CoordinatorInstance coordinator)
    {
        _coordinator = coordinator;
    }

    public Task<HandleMessageResult> HandleMessage(BaseMessage message)
    {
        try
        {
            if (message.Data is UsersInfoRequestModel infoRequest)
            {
                HandleUsersInfo(infoRequest, message);
            }
            else
            {
                return Task.FromResult(HandleMessageResult.ForbiddenMessage);
            }
        }
        catch (JsonException ex)
        {
            SendResponse(new UsersInfoRequestModel(false), UsersRequestResult.InternalError, message);
            return Task.FromResult(HandleMessageResult.JsonParseError);
        }

        return Task.FromResult(HandleMessageResult.NoError);
    }

    private void HandleUsersInfo(UsersInfoRequestModel infoRequest, BaseMessage message)
    {
        foreach (var userId in infoRequest.RequestedUsersList)
        {
            if (_coordinator.GetUser(userId, out var user))
            {
                infoRequest.AddUser(userId, user);
            }
            else
            {
                infoRequest.AddUser(userId, null);
            }
        }
        SendResponse(infoRequest, UsersRequestResult.NoError, message);
    }
        
    private void SendResponse(UsersInfoRequestModel request, UsersRequestResult result, BaseMessage message)
    {
        request.Result = result;
        _coordinator.SendMessageToUser(message.UserId!, new BaseMessage(message)
        {
            Data = request
        });
    }
}