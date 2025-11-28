using Fleck;
using Shared.Models;
using Shared.Models.DTO;

namespace Server.MainServer.Main.Server.Coordinator.Users.Manager;

public interface IUserManager
{
    public int ConnectedUsersCount { get; }

    bool AddUser(UserDTO user, out UserDTO addedUser);
    void RemoveUser(string userId);
    bool GetUser(string userId, out UserDTO? user);
    
    void RemoveAllUsers();
}