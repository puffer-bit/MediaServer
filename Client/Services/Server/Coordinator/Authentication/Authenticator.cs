using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Client.Services.Server.Coordinator.Connection;
using Client.Services.Server.Coordinator.Messaging;
using Shared.Enums;
using Shared.Models;
using Shared.Models.Requests;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared.Models.DTO;

namespace Client.Services.Server.Coordinator.Authentication;

public class Authenticator : IAuthenticator
{
    private readonly CoordinatorSession _coordinatorSession;
    private readonly ResponseAwaiter _awaiter;
    
    public Authenticator(ResponseAwaiter awaiter, CoordinatorSession coordinatorSession)
    {
        _awaiter = awaiter;
        _coordinatorSession = coordinatorSession;
    }

    public async Task<UserAuthRequestModel.AuthStatus> AuthenticateAsync(UserDTO user)
    {
        var message = new BaseMessage(MessageType.UserAuth, user);
        _coordinatorSession.SendMessage(message);
        var response = await _awaiter.WaitForResponseAsync(message.MessageId, message.Type);
        
        UserAuthRequestModel auth = (UserAuthRequestModel)response.Data;

        if (auth.Status == UserAuthRequestModel.AuthStatus.Completed)
        {
            _coordinatorSession.SetUser(auth.UserDto!);
            _coordinatorSession.SetCoordinatorInstanceData(auth.CoordinatorSessionData!);
        }

        return auth.Status;
    }
}