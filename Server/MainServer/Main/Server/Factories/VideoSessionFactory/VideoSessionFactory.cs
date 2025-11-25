using Server.MainServer.Main.Server.Coordinator;
using Server.MainServer.Main.Server.Coordinator.Sessions.Video;
using Server.MainServer.Main.Server.Coordinator.Sessions.Video.PeerManager;
using Shared.Models;

namespace Server.MainServer.Main.Server.Factories.VideoSessionFactory;

public class VideoSessionFactory : IVideoSessionFactory
{
    private readonly IServiceProvider _provider;

    public VideoSessionFactory(IServiceProvider provider)
    {
        _provider = provider;
    }
    
    public IVideoSession CreateVideoSession(VideoSessionDTO dto, CoordinatorInstance coordinator) =>
        ActivatorUtilities.CreateInstance<VideoSession>(_provider, CreateVideoSessionContext(dto), coordinator, dto.IsAudioRequested, dto.IsHostMustApprove);
    [Obsolete] public IVideoSession CreateVideoSession(string id, string name, string hostId, int capacity, bool isAudioRequested, CoordinatorInstance coordinator) =>
        ActivatorUtilities.CreateInstance<VideoSession>(_provider, CreateVideoSessionContext(id, name, hostId, capacity), coordinator, isAudioRequested);
    public IVideoSessionContext CreateVideoSessionContext(VideoSessionDTO dto) =>
        ActivatorUtilities.CreateInstance<VideoSessionContext>(_provider, dto);
    [Obsolete] public IVideoSessionContext CreateVideoSessionContext(string id, string name, string hostId, int capacity) =>
        ActivatorUtilities.CreateInstance<VideoSessionContext>(_provider, id, name, hostId, capacity);
    
    public IVideoPeerManager CreatePeerManager(CoordinatorInstance coordinator, IVideoSession videoSession) =>
        ActivatorUtilities.CreateInstance<VideoPeerManager>(_provider, videoSession, CreatePeerManagerContext(), coordinator);
    public IVideoPeerManagerContext CreatePeerManagerContext() =>
        ActivatorUtilities.CreateInstance<VideoPeerManagerContext>(_provider);
}