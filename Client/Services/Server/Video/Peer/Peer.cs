using SIPSorcery.Net;

namespace Client.Services.Server.Video.Peer;

public class Peer : IPeer
{
    public string? PeerId { get; set; }
    public RTCPeerConnection? PeerConnection { get; set; }
    public MediaStreamTrack? _videoTrack { get; set; }
    public MediaStreamTrack? _audioTrack { get; set; }
    
    public Peer(string? peerId)
    {
        PeerId = peerId;
    }
}