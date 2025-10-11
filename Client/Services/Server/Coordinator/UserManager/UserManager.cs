using Shared.Enums;
using Shared.Models;

namespace Client.Services.Server.Coordinator.UserManager;

public class UserManager : IUserManager
{
    public UserDTO? User { get; set; }
    public UserState State { get; set; }
    
    public void SetUser(UserDTO user)
    {
        User = user;
    }

    public void RemoveUser()
    {
        
    }
    
    public string? GetUserId()
    {
        return User?.Id;
    }

    public UserDTO? GetUser()
    {
        return User;
    }
}