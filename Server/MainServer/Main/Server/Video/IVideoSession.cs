using Server.MainServer.Main.Server.Video.Peer;
using Shared.Enums;
using Shared.Models;
using Shared.Models.DTO;

namespace Server.MainServer.Main.Server.Video;

public interface IVideoSession
{
    public bool IsAudioRequested { get; set; }
    public bool IsHostConnected { get; set; }
    public event Action HostStatusChanged;
    
    VideoSessionDTO AsModel();
    void CreateAndAttachPeer(string userId);
    void AttachPeer(IPeer peer);
    void DetachPeer(string? peerId);
    void ChangePeerApproveState(string? peerId, VideoSessionApproveState newState);
    
    bool GetPeerById(string? peerId, out IPeer? peer);
    bool GetPeerByUserId(string userId, out IPeer? peer);
    List<IPeer> GetAllPeers();
    List<PeerDTO> GetAllPeersAsModel();
    List<UserDTO> GetAllPeersAsUsers();
    List<UserDTO> GetAllPeersAsUsersModel();
    string GetSessionId();
    string? GetHostId();
    string? GetHostPeerId();
    int GetSessionCapacity();
    void SetHostPeerId(string? hostPeerId);
    void ChangeHostState(bool isHostConnected);
}