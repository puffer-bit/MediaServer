using System.Threading.Tasks;
using Shared.Models;
using Shared.Models.DTO;
using Shared.Models.Requests;
using Websocket.Client;

namespace Client.Services.Server.Coordinator.Authentication;

public interface IAuthenticator
{
    Task<UserAuthRequestModel.AuthStatus> AuthenticateAsync(UserDTO user);
}