using SIPSorcery.Net;

namespace Server.MainServer.Main.Server.Coordinator;

public partial class CoordinatorInstance
{
    public void CreateNewWebRTCPeer(string userId, string roomId, string peerId, bool isStreamHost, bool isAudioRequested)
    {
        if (_sessionManager.GetVideoSession(roomId, out _))
        {
            if (_webRTCManager.GetWebRTCPeer(peerId, out _))
            {
                _webRTCManager.DestroyPeerConnection(peerId);
            }
            _webRTCManager.CreatePeerConnection(userId, roomId, peerId, isStreamHost,
                isAudioRequested, false);
        }
    }
    
    public void DisposeWebRTCPeer(string peerId)
    {
        _webRTCManager.DestroyPeerConnection(peerId);
    }

    public void ReactOnOffer(string peerId, RTCSessionDescriptionInit sdpOffer)
    {
        _webRTCManager.ReactOnOffer(peerId, sdpOffer);
    }
    
    public async Task<RTCSessionDescriptionInit> ReactOnOfferRequest(string peerId)
    {
        return await _webRTCManager.ReactOnOfferRequest(peerId);
    }
    
    public void ReactOnAnswer(string peerId, RTCSessionDescriptionInit answerDesc)
    {
        _webRTCManager.ReactOnAnswer(peerId, answerDesc);
    }

    public void ReactOnICE(string peerId, RTCIceCandidateInit candidate)
    {
        _webRTCManager.ReactOnICE(peerId, candidate);
    }
}