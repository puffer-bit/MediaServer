using Shared.Enums;
using Shared.Models;

namespace Server.MainServer.Main.Server.Coordinator.Sessions.Voice;

public class VoiceSession
{
    public required string Id { get; init; }
    public required string Name { get; set; }
    public required string HostId { get; init; }
    public required int Capacity { get; set; }
    
    public readonly CoordinatorInstance _coordinator;
    
    /// <summary>
    /// Convert Session to SessionModel
    /// </summary>
    /// <returns> RoomModel </returns>
    public VoiceSessionDTO AsModel() =>
        new VoiceSessionDTO()
        {
            CoordinatorInstanceId = _coordinator.Context.Id,
            Id = this.Id,
            Name = this.Name,
            Capacity = this.Capacity,
            HostId = this.HostId,
            SessionType = SessionType.Voice
        };
}