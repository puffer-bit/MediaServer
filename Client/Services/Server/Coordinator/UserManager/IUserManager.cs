using Shared.Enums;
using Shared.Models;

namespace Client.Services.Server.Coordinator.UserManager;

public interface IUserManager
{
    UserDTO? User { get; }
    UserState State { get; }
    void SetUser(UserDTO user);
    string? GetUserId();
    UserDTO? GetUser();
}