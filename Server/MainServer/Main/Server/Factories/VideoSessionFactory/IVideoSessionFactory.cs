using Server.MainServer.Main.Server.Coordinator;
using Server.MainServer.Main.Server.Video;
using Server.MainServer.Main.Server.Video.PeerManager;
using Shared.Models;

namespace Server.MainServer.Main.Server.Factories.VideoSessionFactory;

public interface IVideoSessionFactory
{
    IVideoSession CreateVideoSession(VideoSessionDTO dto, CoordinatorInstance coordinator);
    IVideoSessionContext CreateVideoSessionContext(VideoSessionDTO dto);
    
    IVideoPeerManager CreatePeerManager(CoordinatorInstance coordinator, string roomId);
    IVideoPeerManagerContext CreatePeerManagerContext();
}