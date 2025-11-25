using Server.MainServer.Main.Server.Coordinator.Sessions.Video.Peer;

namespace Server.MainServer.Main.Server.Factories.PeerManagerFactory;

public class PeerManagerFactory : IPeerManagerFactory
{
    private readonly IServiceProvider _provider;

    public PeerManagerFactory(IServiceProvider provider)
    {
        _provider = provider;
    }
    
    public IPeer CreatePeer(string id, string userId, bool isAudioRequested) =>
        ActivatorUtilities.CreateInstance<Peer>(_provider, CreatePeerContext(id, userId), isAudioRequested);

    public IPeerContext CreatePeerContext(string id, string userId) =>
        ActivatorUtilities.CreateInstance<PeerContext>(_provider, id, userId);
}