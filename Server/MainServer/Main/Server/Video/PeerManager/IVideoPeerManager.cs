using Server.MainServer.Main.Server.Video.Peer;
using Shared.Models;

namespace Server.MainServer.Main.Server.Video.PeerManager;

public interface IVideoPeerManager
{
    string RoomId { get; set; }

    IPeer CreateNewPeer(string userId, string peerId, SessionDTO sessionDTO);
    void AddPeer(IPeer peer);
    bool RemovePeer(string? peerId);
    IVideoPeerManagerContext GetPeerManagerContext();

    List<IPeer> GetAllPeers();
    List<UserDTO> GetAllPeersAsUsers();
    bool GetPeerById(string? peerId, out IPeer? videoPeer);
    bool GetPeerByUserId(string userId, out IPeer? videoPeer);
}