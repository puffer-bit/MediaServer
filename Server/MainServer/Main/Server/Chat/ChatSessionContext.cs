using Shared.Enums;
using Shared.Models;

namespace Server.MainServer.Main.Server.Chat;

public partial class ChatSessionContext
{
    /// <summary>
    /// Convert Session to SessionModel
    /// </summary>
    /// <returns> RoomModel </returns>
    public ChatSessionDTO AsModel() =>
        new ChatSessionDTO()
        {
            Id = this.Id,
            Name = this.Name,
            Capacity = this.Capacity,
            HostId = this.HostId,
            SessionType = SessionType.Chat
        };
}