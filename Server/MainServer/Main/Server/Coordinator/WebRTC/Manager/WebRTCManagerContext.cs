using System.Collections.Concurrent;
using SIPSorcery.Net;

namespace Server.MainServer.Main.Server.Coordinator.WebRTC.Manager;

public class WebRTCManagerContext : IWebRTCManagerContext
{
    public ConcurrentDictionary<string, RTCPeerConnection> Connections { get; } = new();
    public ConcurrentDictionary<string, MediaStreamTrack> AudioTracks { get; } = new();
    public ConcurrentDictionary<string, MediaStreamTrack> VideoTracks { get; } = new();
    
    
}