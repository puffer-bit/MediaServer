using Shared.Models;

namespace Server.MainServer.Main.Server.Coordinator.Users.Manager;

public interface IUserManagerContext
{
    Dictionary<string, UserDTO?> ConnectedUsers { get; init; }
}