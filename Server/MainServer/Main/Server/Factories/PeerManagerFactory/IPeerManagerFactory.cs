using Server.MainServer.Main.Server.Video.Peer;

namespace Server.MainServer.Main.Server.Factories.PeerManagerFactory;

public interface IPeerManagerFactory
{
    IPeer CreatePeer(string id, string userId, bool isAudioRequested);
    IPeerContext CreatePeerContext(string id, string userId);
}