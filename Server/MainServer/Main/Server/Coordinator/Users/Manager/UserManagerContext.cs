using Shared.Models;

namespace Server.MainServer.Main.Server.Coordinator.Users.Manager;

public class UserManagerContext : IUserManagerContext
{
    public Dictionary<string, UserDTO?> ConnectedUsers { get; set; } = new();
}