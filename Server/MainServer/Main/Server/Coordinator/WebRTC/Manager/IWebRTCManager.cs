using SIPSorcery.Net;

namespace Server.MainServer.Main.Server.Coordinator.WebRTC.Manager;

public interface IWebRTCManager
{
    RTCPeerConnection CreatePeerConnection(string userId, string roomId, string? peerId, bool isStreamHost, bool isAudioRequested, bool isTest);
    void DestroyPeerConnection(string? peerId);
    void ReactOnAnswer(string? peerId, RTCSessionDescriptionInit? answerDesc);
    void ReactOnOffer(string? peerId, RTCSessionDescriptionInit? offerDesc);
    Task<RTCSessionDescriptionInit> ReactOnOfferRequest(string? peerId);
    void ReactOnICE(string? peerId, RTCIceCandidateInit? candidate);
    void ReactOnICE(string? peerId, RTCIceCandidate? iceCandidate);
    bool GetWebRTCPeer(string peerId, out RTCPeerConnection? peerConnection);
}
