using System.Collections.Concurrent;
using Server.MainServer.Main.Server.Video.Peer;

namespace Server.MainServer.Main.Server.Video.PeerManager;

public interface IVideoPeerManagerContext
{
    ConcurrentDictionary<string?, IPeer> Peers { get; init; }
}