using Shared.Enums;
using Shared.Models;
using Shared.Models.DTO;

namespace Client.Services.Server.Coordinator.UserManager;

public interface IUserManager
{
    UserDTO? User { get; }
    UserState State { get; }
    void SetUser(UserDTO user);
    string? GetUserId();
    UserDTO? GetUser();
}