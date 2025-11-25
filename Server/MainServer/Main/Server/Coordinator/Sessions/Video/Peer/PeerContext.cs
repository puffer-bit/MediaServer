namespace Server.MainServer.Main.Server.Coordinator.Sessions.Video.Peer;

public class PeerContext : IPeerContext
{
    public required string Id { get; init; }
    public required string UserId { get; init; }
        
    public PeerContext(string? id, string userId)
    {
        Id = id;
        UserId = userId;
    }
}