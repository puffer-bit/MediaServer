using Shared.Models;
using Shared.Models.DTO;

namespace Server.MainServer.Main.Server.Coordinator.Users.Manager;

public interface IUserManagerContext
{
    Dictionary<string, UserDTO?> ConnectedUsers { get; init; }
}