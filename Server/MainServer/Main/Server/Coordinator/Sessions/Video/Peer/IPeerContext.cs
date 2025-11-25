namespace Server.MainServer.Main.Server.Coordinator.Sessions.Video.Peer;

public interface IPeerContext
{
    string Id { get; init; }
    string UserId { get; init; }
}