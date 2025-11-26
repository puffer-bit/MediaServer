using Server.MainServer.Main.Server.Coordinator.Sessions.Video.Peer;
using Shared.Enums;
using Shared.Models;
using Shared.Models.DTO;

namespace Server.MainServer.Main.Server.Coordinator.Sessions.Video;

public interface IVideoSession
{
    public bool IsAudioRequested { get; set; }
    public bool IsHostMustApprove { get; set; }
    public bool IsHostConnected { get; set; }

    VideoSessionDTO AsModel();
    IPeer CreatePeer(string userId);
    JoinSessionResult AttachPeer(IPeer peer);
    ApproveUserSessionResult ApprovePeerByUserId(string userId);
    ApproveUserSessionResult ApprovePeerByPeerId(string peerId);
    RejectUserSessionResult RejectPeerByUserId(string userId);
    RejectUserSessionResult RejectPeerByPeerId(string peerId);
    KickFromSessionResult DetachPeer(string peerId, bool isForce);
    void DetachAllPeers();

    bool GetPeerById(string? peerId, out IPeer? peer);
    bool GetPeerByUserId(string userId, out IPeer? peer);
    List<IPeer> GetAllPeers();
    List<PeerDTO> GetAllPeersAsModel();
    List<UserDTO> GetAllPeersAsUsers();
    List<UserDTO> GetAllPeersAsUsersModel();
    string GetSessionId();
    string GetName();
    string? GetHostId();
    string? GetHostPeerId();
    int GetSessionCapacity();
    void SetHostPeerId(string? hostPeerId);
    void ChangeHostState(bool isHostConnected);
    int GetAllPeersCount();
    
    void CloseSession();
}