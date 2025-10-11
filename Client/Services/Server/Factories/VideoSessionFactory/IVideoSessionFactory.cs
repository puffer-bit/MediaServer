using Client.Services.Server.Coordinator;
using Client.Services.Server.Video;
using Client.Services.Server.Video.Peer;
using Shared.Models;

namespace Client.Services.Server.Factories.VideoSessionFactory;

public interface IVideoSessionFactory
{
    void Initialize(CoordinatorSession coordinator);
    
    IVideoSession CreateVideoSession(VideoSessionDTO videoSessionDTO, string? peerId);
    IPeer CreatePeer(string? peerId);
}