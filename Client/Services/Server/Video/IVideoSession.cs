using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Client.Services.Server.Video.Peer;
using Shared.Enums;
using Shared.Models;
using SIPSorcery.Net;
using SIPSorceryMedia.Abstractions;

namespace Client.Services.Server.Video;

public interface IVideoSession : IDisposable
{
    IPeer Peer { get; }
    string Id { get; set; }
    string? HostId { get; set; }
    string Name { get; set; }
    int Capacity { get; set; }
    string? HostPeerId { get; set; }
    bool IsHostConnected { get; set; }
    bool IsAudioRequested { get; set; }
    bool IsHost { get; }
    VideoSessionState State { get; set; }
    Queue<RTCIceCandidateInit> IceCandidatesBuffer { get; }
    event Action<WriteableBitmap>? FrameReceived;

    VideoSessionDTO AsModel();
    void HandleHostConnected();
    void HandleHostDissconnected();
    Task<WebRTCNegotiationResult> Renegotiate();
    Task RefreshSession();
    Task<WebRTCNegotiationResult> Negotiate();
    void StartReceive();
    void EndReceive();
    Task<bool> StartSending();
    Task StopSending();
}