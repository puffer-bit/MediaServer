using Server.MainServer.Main.Server.Video.Peer;
using Shared.Models;

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
    
    bool GetPeerById(string? peerId, out IPeer? peer);
    bool GetPeerByUserId(string userId, out IPeer? peer);
    List<IPeer> GetAllPeers();
    List<UserDTO> GetAllPeersAsUsers();
    string GetSessionId();
    string? GetHostId();
    string? GetHostPeerId();
    int GetSessionCapacity();
    void SetHostPeerId(string? hostPeerId);
    void ChangeHostState(bool isHostConnected);
}