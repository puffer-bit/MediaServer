using Shared.Models;
using Shared.Models.DTO;

namespace Server.MainServer.Main.Server.Chat;

public partial class ChatSessionContext
{
    public required string Id { get; init; }
    public required string Name { get; set; }
    public required string HostId { get; init; }
    public required int Capacity { get; set; }
    public required Mutex Mutex { get; set; }
    private List<UserDTO> AllowedUsers { get; set; } = [];
}