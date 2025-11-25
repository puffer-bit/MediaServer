using Shared.Models;

namespace Server.MainServer.Main.Server.Coordinator.Sessions.Video;

public class VideoSessionContext : IVideoSessionContext
{
    public required string Id { get; init; }
    public required string Name { get; set; }
    public required int Capacity { get; set; }
    public required string? HostId { get; set; }
    public string? HostPeerId { get; set; }
    
    public VideoSessionContext(VideoSessionDTO dto)
    {
        Id = dto.Id;
        Name = dto.Name;
        Capacity = dto.Capacity;
        HostId = dto.HostId;
    }
    
    public VideoSessionContext(string id, string name, int capacity, string? hostId)
    {
        Id = id;
        Name = name;
        Capacity = capacity;
        HostId = hostId;
    }
}