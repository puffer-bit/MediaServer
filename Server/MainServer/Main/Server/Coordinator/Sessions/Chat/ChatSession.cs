using Shared.Enums;
using Shared.Models;

namespace Server.MainServer.Main.Server.Coordinator.Sessions.Chat;

public partial class ChatSession
{
    
    public readonly CoordinatorInstance _coordinator;

    /// <summary>
    /// Convert Session to SessionModel
    /// </summary>
    /// <returns> RoomModel </returns>
    public ChatSessionDTO AsModel() =>
        new ChatSessionDTO()
        {
            CoordinatorInstanceId = _coordinator.Context.Id,
            Id = this.Id,
            Name = this.Name,
            Capacity = this.Capacity,
            HostId = this.HostId,
            SessionType = SessionType.Chat
        };
}