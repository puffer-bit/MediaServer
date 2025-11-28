using System.Net.Sockets;
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
    
    public async Task CheckTurnAndStunServer()
    {
        if (Context.IsStunEnabled)
        {
            if (await CheckStunServer())
            {
                IsStunServerAvailable = true;
                _logger.LogInformation("STUN server {Ip}:{Port} available.", Context.StunAddress, Context.StunPort);
            }
            else
            {
                IsStunServerAvailable = false;
                _logger.LogWarning("STUN server {Ip}:{Port} unavailable.", Context.StunAddress, Context.StunPort);
            }
        }

        if (Context.IsTurnEnabled)
        {
            if (await CheckTurnServer())
            {
                IsTurnServerAvailable = true;
                _logger.LogInformation("TURN server {Ip}:{Port} available.", Context.TurnAddress, Context.TurnPort);
            }
            else
            {
                IsTurnServerAvailable = false;
                _logger.LogWarning("TURN server {Ip}:{Port} unavailable.", Context.TurnAddress, Context.TurnPort);
            }
        }
    }
    
    private async Task<bool> CheckStunServer()
    {
        if (Context.IsStunEnabled)
        {
            try
            {
                using var udpClient = new UdpClient();
                udpClient.Connect(Context.StunAddress!, (int)Context.StunPort!);

                byte[] stunRequest = new byte[20];
                stunRequest[4] = 0x21;
                stunRequest[5] = 0x12;
                stunRequest[6] = 0xA4;
                stunRequest[7] = 0x42;

                await udpClient.SendAsync(stunRequest, stunRequest.Length);

                var receiveTask = udpClient.ReceiveAsync();
                var completed = await Task.WhenAny(receiveTask, Task.Delay(3000));

                if (completed == receiveTask)
                {
                    var result = receiveTask.Result;
                    return result.Buffer.Length > 0;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
        
        return false;
    }
    
    private async Task<bool> CheckTurnServer()
    {
        if (Context.IsTurnEnabled)
        {
            using var tcpClient = new TcpClient();

            try
            {
                var connectTask = tcpClient.ConnectAsync(Context.TurnAddress!, (int)Context.TurnPort!);
                var timeoutTask = Task.Delay(3000);

                var completed = await Task.WhenAny(connectTask, timeoutTask);

                if (completed == connectTask)
                {
                    await connectTask;
                    return true; 
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        return false;
    }
}