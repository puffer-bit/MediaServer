using System.Collections.Concurrent;
using Server.MainServer.Main.Server.Coordinator.Sessions.Video.Peer;

namespace Server.MainServer.Main.Server.Coordinator.Sessions.Video.PeerManager;

public interface IVideoPeerManagerContext
{
    ConcurrentDictionary<string, IPeer> Peers { get; init; }
}