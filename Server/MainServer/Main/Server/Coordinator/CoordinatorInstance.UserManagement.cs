using Fleck;
using Shared.Enums;
using Shared.Models;
using Shared.Models.DTO;
using Shared.Models.Requests;
using Shared.Models.Requests.Heartbeat;

namespace Server.MainServer.Main.Server.Coordinator;

public partial class CoordinatorInstance
{
    public bool AddUserToInstance(UserDTO user, out UserDTO? addedUser)
    {
        _userManager.AddUser(user, out var newUser);
        addedUser = newUser;
        return true;
    }

    public void RemoveUserFromInstance(string userId)
    {
        _userManager.RemoveUser(userId);
    }
    
    public void AttachUser(BaseMessage message, IWebSocketConnection webSocket)
    {
        var userDTO = (UserDTO)message.Data;
        if (AddUserToInstance(userDTO, out var addedUser))
        {
            _heartbeatManager.StartAsync(userDTO.Id!, AddNewConnection(userDTO.Id!, webSocket).Token);
            SendMessageToUser(userDTO.Id!, new BaseMessage(MessageType.UserAuth, new UserAuthRequestModel(UserAuthRequestModel.AuthStatus.Completed, addedUser!, Context.Id))
            {
                MessageId = message.MessageId
            });
        }
        else
        {
            SendMessageToUser(userDTO.Id!, new BaseMessage(MessageType.UserAuth, new UserAuthRequestModel(UserAuthRequestModel.AuthStatus.WrongLoginData, addedUser!, Context.Id))
            {
                MessageId = message.MessageId
            });
        }
    }
    
    public void DetachUser(string userId, string reason, BaseMessage? disconnectMessage = null)
    {
        if (disconnectMessage != null)
            SendMessageToUser(userId, new BaseMessage(MessageType.Heartbeat, new HeartbeatModel(HeartbeatType.Disconnected)));
         
        GetUser(userId, out var user);
        RemoveUserFromAllSessions(userId);
        RemoveUserFromInstance(userId);
        RemoveConnection(userId);
        _logger.LogInformation("User {Name} disconnected. Reason: {Reason}", user?.Username, reason);
    }

    public bool GetUser(string userId, out UserDTO? user)
    {
        if (_userManager.GetUser(userId, out var foundedUser))
        {
            user = foundedUser;
            return true;
        }
        user = null;
        return false;
    }
}