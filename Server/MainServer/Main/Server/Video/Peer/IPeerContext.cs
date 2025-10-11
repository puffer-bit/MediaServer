namespace Server.MainServer.Main.Server.Video.Peer;

public interface IPeerContext
{
    string? Id { get; init; }
    string UserId { get; init; }
}