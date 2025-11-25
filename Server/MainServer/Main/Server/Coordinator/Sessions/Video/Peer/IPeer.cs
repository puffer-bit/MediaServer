using Shared.Enums;

namespace Server.MainServer.Main.Server.Coordinator.Sessions.Video.Peer;

public interface IPeer
{
    public bool IsStreamHost { get; set; }
    public bool IsAudioRequested { get; set; }
    public VideoSessionPeerState PeerState { get; set; }
    public bool IsNegotiated { get; set; }
    public bool IsConnected { get; set; }
    public bool IsApproved { get; set; }
    public bool IsRejected { get; set; }
    
    string GetId();
    string GetUserId();
    void MakeHost();
}