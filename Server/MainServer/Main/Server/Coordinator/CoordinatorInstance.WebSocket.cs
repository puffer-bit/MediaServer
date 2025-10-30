using Fleck;
using Newtonsoft.Json.Linq;
using Server.MainServer.Main.Server.Coordinator.Connection;
using Shared.Enums;
using Shared.Models;
using Shared.Models.DTO;
using Shared.Models.Requests;
using Shared.Models.Requests.Heartbeat;

namespace Server.MainServer.Main.Server.Coordinator;

public partial class CoordinatorInstance
{
    public CancellationTokenSource AddNewConnection(string userId, IWebSocketConnection webSocket)
    {
        return _connectionManager.Add(userId, webSocket);
    }
    
    public void RemoveConnection(string userId)
    {
        _connectionManager.Remove(userId);
    }

    public bool GetClientConnection(string userId, out IClientConnection? foundedClientConnection)
    {
        if (_connectionManager.Get(userId, out var clientConnection))
        {
            foundedClientConnection = clientConnection!;
            return true;
        }

        foundedClientConnection = null;
        return false;
    }

    public IDictionary<string, IClientConnection> GetAllConnections()
    {
        return _connectionManager.GetAll();
    }

    public void RegisterPong(string userId)
    {
        _heartbeatManager.RegisterPong(userId);
    }
    
    public void AttachUser(BaseMessage message, IWebSocketConnection webSocket)
    {
        var userDTO = (UserDTO)message.Data;
        if (AddUserToInstance(userDTO, out var addedUser))
        {
            _heartbeatManager.StartAsync(userDTO.Id!, AddNewConnection(userDTO.Id!, webSocket).Token);
            SendMessageToUser(userDTO.Id!, new BaseMessage(MessageType.UserAuth, new UserAuthRequestModel(UserAuthRequestModel.AuthStatus.Completed, addedUser!))
            {
                MessageId = message.MessageId
            });
        }
        else
        {
            SendMessageToUser(userDTO.Id!, new BaseMessage(MessageType.UserAuth, new UserAuthRequestModel(UserAuthRequestModel.AuthStatus.WrongLoginData, addedUser!))
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
}