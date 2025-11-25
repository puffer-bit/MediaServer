using Server.MainServer.Main.Server.Coordinator.Sessions.Video.Peer;
using Shared.Enums;
using Shared.Models;
using Shared.Models.DTO;

namespace Server.MainServer.Main.Server.Coordinator.Sessions.Video.PeerManager;

public interface IVideoPeerManager
{
    IPeer CreateNewPeer(string userId, string peerId, VideoSessionDTO sessionDTO);
    JoinSessionResult AddPeer(IPeer peer, bool isForce = false, bool isSilent = false);
    ApproveUserSessionResult ApprovePeer(string peerId);
    RejectUserSessionResult RejectPeer(string peerId);
    KickFromSessionResult RemovePeer(string peerId, bool isForce, bool isSilent = false);
    
    IVideoPeerManagerContext GetPeerManagerContext();

    List<IPeer> GetAllPeers();
    List<UserDTO> GetAllPeersAsUsers();
    int GetAllPeersCount();
    bool GetPeerById(string? peerId, out IPeer? videoPeer);
    bool GetPeerByUserId(string userId, out IPeer? videoPeer);
    void RemoveAllPeers();
}