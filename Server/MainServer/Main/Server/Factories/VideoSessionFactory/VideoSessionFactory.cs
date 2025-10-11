using Server.MainServer.Main.Server.Coordinator;
using Server.MainServer.Main.Server.Video;
using Server.MainServer.Main.Server.Video.PeerManager;
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
        ActivatorUtilities.CreateInstance<VideoSession>(_provider, CreateVideoSessionContext(dto), CreatePeerManager(coordinator, dto.Id), coordinator, dto.IsAudioRequested);
    [Obsolete] public IVideoSession CreateVideoSession(string id, string name, string hostId, int capacity, bool isAudioRequested, CoordinatorInstance coordinator) =>
        ActivatorUtilities.CreateInstance<VideoSession>(_provider, CreateVideoSessionContext(id, name, hostId, capacity), CreatePeerManager(coordinator, id), coordinator, isAudioRequested);
    public IVideoSessionContext CreateVideoSessionContext(VideoSessionDTO dto) =>
        ActivatorUtilities.CreateInstance<VideoSessionContext>(_provider, dto);
    [Obsolete] public IVideoSessionContext CreateVideoSessionContext(string id, string name, string hostId, int capacity) =>
        ActivatorUtilities.CreateInstance<VideoSessionContext>(_provider, id, name, hostId, capacity);
    
    public IVideoPeerManager CreatePeerManager(CoordinatorInstance coordinator, string roomId) =>
        ActivatorUtilities.CreateInstance<VideoPeerManager>(_provider, CreatePeerManagerContext(), coordinator, roomId);
    public IVideoPeerManagerContext CreatePeerManagerContext() =>
        ActivatorUtilities.CreateInstance<VideoPeerManagerContext>(_provider);
}