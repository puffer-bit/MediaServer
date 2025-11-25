namespace Server.MainServer.Main.Server.Coordinator.Sessions.Video;

public interface IVideoSessionContext
{
    string Id { get; init; }
    string Name { get; set; }
    int Capacity { get; set; }
    public string? HostId { get; set; }
    public string? HostPeerId { get; set; }
}