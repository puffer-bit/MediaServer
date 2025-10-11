using SIPSorcery.Net;

namespace Client.Services.Server.Video.Peer;

public interface IPeer
{
    string? PeerId { get; set; }
    RTCPeerConnection? PeerConnection { get; set; }
    MediaStreamTrack? _videoTrack { get; set; }
    MediaStreamTrack? _audioTrack { get; set; }
}