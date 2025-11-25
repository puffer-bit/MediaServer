using System.Collections.Concurrent;
using Server.MainServer.Main.Server.Coordinator.Sessions.Video.Peer;

namespace Server.MainServer.Main.Server.Coordinator.Sessions.Video.PeerManager;

public class VideoPeerManagerContext : IVideoPeerManagerContext
{
    public ConcurrentDictionary<string, IPeer> Peers { get; init; } = new();
}