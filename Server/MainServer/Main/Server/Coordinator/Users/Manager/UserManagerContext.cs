using Shared.Models;
using Shared.Models.DTO;

namespace Server.MainServer.Main.Server.Coordinator.Users.Manager;

public class UserManagerContext : IUserManagerContext
{
    public Dictionary<string, UserDTO?> ConnectedUsers { get; init; } = new();
}