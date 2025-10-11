using Fleck;
using Shared.Models;

namespace Server.MainServer.Main.Server.Coordinator.Users.Manager;

public interface IUserManager
{
    bool AddUser(UserDTO user, out UserDTO addedUser);
    void RemoveUser(string userId);
    bool GetUser(string userId, out UserDTO? user);
}