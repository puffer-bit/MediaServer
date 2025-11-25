using System;
using System.Threading.Tasks;
using Shared.Enums;
using Shared.Models;
using Shared.Models.DTO;
using Shared.Models.Requests;
using Shared.Models.Requests.Users;
using SIPSorcery.Net;

namespace Client.Services.Server.Coordinator;

public partial class CoordinatorSession
{
    public UserDTO GetUser()
    {
        return _userManager.GetUser();
    }

    public async Task<UserDTO?> GetRemoteUser(string userId)
    {
        var usersInfoRequest = new UsersInfoRequestModel(false);
        usersInfoRequest.AddRequestedUser(userId);
        
        var message = new BaseMessage(GetUser().Id, MessageType.UserInfoRequest, usersInfoRequest);
        SendMessage(message);
        
        var response = await _responseAwaiter.WaitForResponseAsync(message.MessageId, message.Type);
        var requestAnswer = (UsersInfoRequestModel)response.Data;
        if (requestAnswer.Result is UsersRequestResult.NoError)
        {
            if (requestAnswer.UsersData.TryGetValue(userId, out var userDTO))
            {
                return userDTO;
            }
        }

        return null;
    }

    public void SetUser(UserDTO user)
    {
        _userManager.SetUser(user);
    }
}