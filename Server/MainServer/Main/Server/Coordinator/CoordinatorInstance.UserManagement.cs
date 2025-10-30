using Fleck;
using Shared.Models;
using Shared.Models.DTO;

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