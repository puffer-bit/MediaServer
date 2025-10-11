using System.Net;
using Server.MainServer.Main.Server.Video;
using Shared.Enums;
using Shared.Models;
using Shared.Models.Requests;
using SIPSorcery.Net;
using SIPSorceryMedia.Abstractions;

namespace Server.MainServer.Main.Server.Coordinator.WebRTC.Manager;

public class WebRTCManager : IWebRTCManager
{
    private readonly Dictionary<string?, Action<IPEndPoint, SDPMediaTypesEnum, RTPPacket>> _clientHandlers = new();
    
    private readonly IWebRTCManagerContext _context;
    private readonly ILogger _logger;
    private readonly CoordinatorInstance _coordinator;

    public WebRTCManager(IWebRTCManagerContext context, 
        ILoggerFactory loggerFactory, CoordinatorInstance coordinator)
    {
        _context = context;
        _logger = loggerFactory.CreateLogger("PeerConnectionManager");
        _coordinator = coordinator;
    }

    /// <summary>
    /// Create WbeRTC peer connection
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="roomId"></param>
    /// <param name="peerId"></param>
    /// <param name="isStreamHost"></param>
    /// <param name="isAudioRequested"></param>
    /// <param name="isTest"></param>
    public RTCPeerConnection CreatePeerConnection(string userId, string roomId, string? peerId, bool isStreamHost, bool isAudioRequested, bool isTest)
    {
        var config = new RTCConfiguration
        {
            iceServers = new List<RTCIceServer>
            {
                new RTCIceServer { urls = "stun:stun.sipsorcery.com" }
            }
        };

        var pc = new RTCPeerConnection(config);

        AddVideoTrack(peerId, pc, isTest);

        if (isAudioRequested)
        {
            AddAudioTrack(peerId, pc);
        }

        if (!isTest && _coordinator.GetVideoSession(roomId, out var videoSession) == SessionRequestResult.NoError)
        {
            RegisterConnectionEvents(pc, userId, roomId, peerId, isStreamHost, videoSession);
            _context.Connections.TryAdd(peerId, pc);
        }

        return pc;
    }
    public void DestroyPeerConnection(string? peerId)
    {
        _context.Connections[peerId].Close("normal");
        _context.VideoTracks.TryRemove(peerId, out _);
        _context.AudioTracks.TryRemove(peerId, out _);
    }
    
    // TODO: Add error answer
    /// <summary>
    ///  Sends answer to PeerConnection
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="roomId"></param>
    /// <param name="answer"></param>
    /// <param name="selfPeerId"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public void SendAnswer(string userId, string roomId, RTCSessionDescriptionInit? answer, string? selfPeerId)
    {
        if (selfPeerId == null)
        {
            throw new ArgumentNullException(nameof(selfPeerId));
        }
        
        WebRTCNegotiation answerMessage = new WebRTCNegotiation(WebRTCNegotiationType.Answer, selfPeerId, roomId, answer);
        _coordinator.SendMessageToUser(userId, new BaseMessage(MessageType.WebRTCInit, answerMessage));
    }

    /// <summary>
    /// Reacts on WebRTC answer
    /// </summary>
    /// <param name="peerId"></param>
    /// <param name="answerDesc"></param>
    /// <returns></returns>
    public void ReactOnAnswer(string? peerId, RTCSessionDescriptionInit? answerDesc)
    {
        if (_context.Connections.TryGetValue(peerId, out var pc) && answerDesc != null)
        {
            pc.setRemoteDescription(answerDesc);
        }
        else
        {
            throw new ApplicationException("React on answer exception.");
        }
    }

    /// <summary>
    /// Subject to change
    /// </summary>
    /// <param name="peerId"></param>
    /// <param name="offerDesc"></param>
    public void ReactOnOffer(string? peerId, RTCSessionDescriptionInit? offerDesc)
    {
        if (_context.Connections.TryGetValue(peerId, out var pc) && offerDesc != null)
        {
            var result = pc.setRemoteDescription(offerDesc);
            if (result != SetDescriptionResultEnum.OK)
            {
                _logger.LogError($"Failed to set remote description, {result}.");
                //DestroyPeerConnection();
            }
        }
        else
        {
            throw new ApplicationException("React on offer exception.");
        }
    }
    
    /// <summary>
    /// Reacts on WebRTC Offer Request
    /// TODO: -
    /// </summary>
    /// <returns></returns>
    public async Task<RTCSessionDescriptionInit> ReactOnOfferRequest(string? peerId)
    {
        if (_context.Connections.TryGetValue(peerId, out var pc))
        {
            RTCSessionDescriptionInit? offerSdp = pc.createOffer();
            await pc.setLocalDescription(offerSdp);
            return offerSdp;
        }

        throw new ApplicationException("React on offer request exception. PeerConnection not created.");
    }

    /// <summary>
    /// Reacts on WebRTC ICE candidate
    /// </summary>
    /// <param name="peerId"></param>
    /// <param name="candidate"></param>
    public void ReactOnICE(string? peerId, RTCIceCandidateInit? candidate)
    {
        if (_context.Connections.TryGetValue(peerId, out var pc) && candidate != null)
        {
            pc.addIceCandidate(candidate);
        }
        else
        {
            throw new ApplicationException("React on ICE candidate exception. PeerConnection not created.");
        }
    }
    public void ReactOnICE(string? peerId, RTCIceCandidate? iceCandidate)
    {
        if (_context.Connections.TryGetValue(peerId, out var pc) && iceCandidate != null)
        {
            var candidateDTO = new RTCIceCandidateInit()
            {
                candidate = iceCandidate.candidate,
                sdpMid = iceCandidate.sdpMid,
                sdpMLineIndex = iceCandidate.sdpMLineIndex
            };
            pc.addIceCandidate(candidateDTO);
        }
        else
        {
            throw new ApplicationException("React on ICE candidate exception. PeerConnection not created.");
        }
    }

    public bool GetWebRTCPeer(string peerId, out RTCPeerConnection? peerConnection)
    {
        if (_context.Connections.TryGetValue(peerId, out var rtcPeerConnection))
        {
            peerConnection = rtcPeerConnection;
            return true;
        }

        peerConnection = null;
        return false;
    }

    private void HostStatusChanged(string? peerId, IVideoSession videoSession)
    {
        if (videoSession.IsHostConnected)
            HandleSendRtp(peerId, videoSession.GetHostPeerId());
        else
            UnHandleSendRtp(peerId, videoSession.GetHostPeerId());
    }
    
    private void HandleSendRtp(string? peerId, string? hostPeerId)
    {
        if (!_context.Connections.TryGetValue(peerId, out var pc))
        {
            _logger.LogWarning($"Peer '{peerId}' not found.");
            return;
        }

        void RtpPacketHandler(IPEndPoint e, SDPMediaTypesEnum media, RTPPacket rtpPkt)
        {
            if (media != SDPMediaTypesEnum.audio && media != SDPMediaTypesEnum.video)
                return;

            //_logger.LogDebug($"Sending RTCP packet to peer {e}.");
            pc!.SendRtpRaw(
                media,
                rtpPkt.Payload,
                rtpPkt.Header.Timestamp,
                rtpPkt.Header.MarkerBit,
                rtpPkt.Header.PayloadType
            );
        }

        if (_clientHandlers.TryAdd(peerId, RtpPacketHandler))
        {
            if (_context.Connections.TryGetValue(hostPeerId, out var hostPc))
                hostPc.OnRtpPacketReceived += _clientHandlers[peerId];
        }
        else
        {
            _logger.LogWarning($"Handler for peer '{peerId}' already exists.");
        }
    }
    
    private void UnHandleSendRtp(string? peerId, string? hostPeerId)
    {
        if (_clientHandlers.TryGetValue(peerId, out Action<IPEndPoint, SDPMediaTypesEnum, RTPPacket>? value))
        {
            if (_context.Connections.TryGetValue(peerId, out var pc))
            {
                pc.OnRtpPacketReceived -= value;
                if (_context.Connections.TryGetValue(peerId, out var hostPc))
                    hostPc.OnRtpPacketReceived -= _clientHandlers[peerId];
            }
            _clientHandlers.Remove(peerId);
        }
    }
    
    private void HandleConnectedState(string? peerId, bool isStreamHost, IVideoSession videoSession, Action hostStatusHandler)
    {
        if (isStreamHost)
        {
            videoSession.ChangeHostState(true);
        }
        else
        {
            if (videoSession.IsHostConnected)
            {
                HandleSendRtp(peerId, videoSession.GetHostPeerId());
            }

            videoSession.HostStatusChanged += hostStatusHandler;
        }
    }

    private void HandleDisconnectedState(string? peerId, bool isStreamHost, IVideoSession videoSession, Action hostStatusHandler)
    {
        if (isStreamHost)
        {
            videoSession.ChangeHostState(false);
        }
        else
        {
            if (_clientHandlers.ContainsKey(peerId))
            {
                UnHandleSendRtp(peerId, videoSession.GetHostPeerId());
            }

            videoSession.HostStatusChanged -= hostStatusHandler;
        }
    }

    private void RegisterConnectionEvents(RTCPeerConnection pc, string userId, string roomId, string? peerId, bool isStreamHost, IVideoSession videoSession)
    {
        var hostStatusHandler = () => HostStatusChanged(peerId, videoSession);

        var addedCandidates = new HashSet<string>();

        pc.onicecandidate += candidate =>
        {
            if (candidate != null && addedCandidates.Add(candidate.candidate))
            {
                var candidateDTO = new RTCIceCandidateInit
                {
                    candidate = candidate.candidate,
                    sdpMid = candidate.sdpMid,
                    sdpMLineIndex = candidate.sdpMLineIndex
                };

                var iceMessage = new WebRTCNegotiation(WebRTCNegotiationType.ICE, peerId, roomId, candidateDTO);
                _coordinator.SendMessageToUser(userId, new BaseMessage(MessageType.WebRTCInit, iceMessage));
            }
        };

        pc.onconnectionstatechange += state =>
        {
            //_logger.LogDebug($"Peer connection state change to {state}.");

            switch (state)
            {
                case RTCPeerConnectionState.connected:
                    HandleConnectedState(peerId, isStreamHost, videoSession, hostStatusHandler);
                    break;

                case RTCPeerConnectionState.failed:
                    HandleDisconnectedState(peerId, isStreamHost, videoSession, hostStatusHandler);
                    _context.Connections.TryRemove(peerId, out _);
                    pc.Dispose();
                    _logger.LogWarning("Peer connection failed.");
                    break;

                case RTCPeerConnectionState.disconnected:
                    HandleDisconnectedState(peerId, isStreamHost, videoSession, hostStatusHandler);
                    pc.Dispose();
                    _context.Connections.TryRemove(peerId, out _);
                    break;
                
                case RTCPeerConnectionState.closed:
                    HandleDisconnectedState(peerId, isStreamHost, videoSession, hostStatusHandler);
                    pc.Dispose();
                    _context.Connections.TryRemove(peerId, out _);
                    break;
            }
        };
    }

    private void AddVideoTrack(string? peerId, RTCPeerConnection pc, bool isTest)
    {
        var videoTrack = new MediaStreamTrack(
            new VideoFormat(96, "H264", 90000, "profile-level-id=42e01f;packetization-mode=1;")
        );

        if (!isTest)
        {
            _context.VideoTracks.TryAdd(peerId, videoTrack);
        }

        pc.addTrack(videoTrack);
    }
    
    private void AddAudioTrack(string? peerId, RTCPeerConnection pc)
    {
        var audioFormats = new List<AudioFormat>
        {
            new AudioFormat
            {
                FormatName = "L16",
                Codec = AudioCodecsEnum.PCM_S16LE,
                ClockRate = 44100,
                RtpClockRate = 44100,
                ChannelCount = 2,
                FormatID = 14
            }
        };

        var audioTrack = new MediaStreamTrack(audioFormats);
        pc.addTrack(audioTrack);
    }
}