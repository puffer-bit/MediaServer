using System.Collections.Concurrent;
using SIPSorcery.Net;

namespace Server.MainServer.Main.Server.Coordinator.WebRTC.Manager;

public interface IWebRTCManagerContext
{
    ConcurrentDictionary<string, RTCPeerConnection> Connections { get; }
    ConcurrentDictionary<string, MediaStreamTrack> AudioTracks { get; }
    ConcurrentDictionary<string, MediaStreamTrack> VideoTracks { get; }
}