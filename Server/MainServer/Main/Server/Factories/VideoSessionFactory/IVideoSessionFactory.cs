using Server.MainServer.Main.Server.Coordinator;
using Server.MainServer.Main.Server.Coordinator.Sessions.Video;
using Server.MainServer.Main.Server.Coordinator.Sessions.Video.PeerManager;
using Shared.Models;

namespace Server.MainServer.Main.Server.Factories.VideoSessionFactory;

public interface IVideoSessionFactory
{
    IVideoSession CreateVideoSession(VideoSessionDTO dto, CoordinatorInstance coordinator);
    IVideoSessionContext CreateVideoSessionContext(VideoSessionDTO dto);
    
    IVideoPeerManager CreatePeerManager(CoordinatorInstance coordinator, IVideoSession videoSession);
    IVideoPeerManagerContext CreatePeerManagerContext();
}