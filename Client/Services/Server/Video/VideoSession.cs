using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Client.Services.Other.AudioPlayerService;
using Client.Services.Other.FrameProcessor;
using Client.Services.Other.ScreenCastService;
using Client.Services.Other.ScreenCastService.XdgDesktopPortalClient;
using Client.Services.Server.Coordinator;
using Client.Services.Server.Video.Peer;
using ReactiveUI;
using Shared.Enums;
using Shared.Models;
using Shared.Models.Requests;
using SIPSorcery.Net;
using SIPSorceryMedia.Abstractions;
using SIPSorceryMedia.FFmpeg;

namespace Client.Services.Server.Video;

public class VideoSession : ReactiveObject, IVideoSession
{
    public IPeer Peer { get; }
    public required string Id { get; set; }
    public required string? HostId { get; set; }
    public required string Name { get; set; }
    public required int Capacity { get; set; }
    public string? HostPeerId { get; set; }
    public required bool IsHostConnected { get; set; }
    public required bool IsAudioRequested { get; set; }
    private VideoSessionState _state;
    public VideoSessionState State
    {
        get => _state;
        set => this.RaiseAndSetIfChanged(ref _state, value);
    }

    public bool IsHost => _coordinatorSession.GetUser().Id == HostId;
    public Queue<RTCIceCandidateInit> IceCandidatesBuffer { get; } = new();
    private readonly CoordinatorSession _coordinatorSession;
    
    // DI
    private readonly IAudioPlayerService? _audioPlayerService;
    private readonly IScreenCastClient _screenCastClient;
    private readonly IFrameProcessor _frameProcessor;
    Stopwatch stopwatch = Stopwatch.StartNew();
    private readonly FFmpegVideoEndPoint _ffmpegVideoEndPoint;
    private VideoFormat? _videoFormat;
    public event Action<WriteableBitmap>? FrameReceived;
    
    public VideoSession(
        VideoSessionDTO sessionDTO, 
        CoordinatorSession coordinatorSession,
        IScreenCastClient screenCastClient,
        IPeer peer, 
        IAudioPlayerService audioPlayerService, 
        IFrameProcessor frameProcessor)
    {
        _coordinatorSession = coordinatorSession;
        Id = sessionDTO.Id;
        HostId = sessionDTO.HostId;
        Name = sessionDTO.Name;
        Capacity = sessionDTO.Capacity;
        HostPeerId = sessionDTO.HostPeerId;
        IsHostConnected = sessionDTO.IsHostConnected;
        IsAudioRequested = sessionDTO.IsAudioRequested;
        Peer = peer;

        if (sessionDTO.IsAudioRequested)
        {
            _audioPlayerService = audioPlayerService;
        }
        _ffmpegVideoEndPoint = new FFmpegVideoEndPoint();
        _frameProcessor = frameProcessor;
        _screenCastClient = screenCastClient; 
        if (HostId == _coordinatorSession.GetUser().Id)
        {
            State = VideoSessionState.WaitingForUserStream;
        }
        else if (IsHostConnected)
        {
            State = VideoSessionState.WaitingForNegotiation;
            _ = Negotiate();
        }
        else
        {
            State = VideoSessionState.WaitingForHost;
        }
    }

    public VideoSessionDTO AsModel()
    {
        return new VideoSessionDTO()
        {
            Id = this.Id,
            HostId = this.HostId,
            Name = this.Name,
            Capacity = this.Capacity,
            HostPeerId = this.HostPeerId,
            IsHostConnected = this.IsHostConnected,
            IsAudioRequested = this.IsAudioRequested,
            SessionType = SessionType.Video
        };
    }

    private void UpdateSessionData(VideoSessionDTO sessionDTO)
    {
        Id = sessionDTO.Id;
        HostId = sessionDTO.HostId;
        Name = sessionDTO.Name;
        Capacity = sessionDTO.Capacity;
        HostPeerId = sessionDTO.HostPeerId;
        IsHostConnected = sessionDTO.IsHostConnected;
        if (IsHostConnected)
            HandleHostConnected();
        else
            HandleHostDissconnected();
        IsAudioRequested = sessionDTO.IsAudioRequested;
    }

    public void HandleHostConnected()
    {
        if (State == VideoSessionState.WaitingForHost)
        {
            if (Peer.PeerConnection == null)
                _ = Negotiate();
            else
                _ = Renegotiate();
        }
    }
    
    public void HandleHostDissconnected()
    {
        if (State == VideoSessionState.Connected)
        {
            if (Peer.PeerConnection != null && Peer.PeerConnection.connectionState == RTCPeerConnectionState.connected)
            {
                Peer.PeerConnection.Close("Host leaved");
                State = VideoSessionState.WaitingForHost;
            }
        }
    }
    
    public async Task RefreshSession()
    {
        SessionDTO? sessionDTO = await _coordinatorSession.GetSessionById(Id);
        if (sessionDTO == null)
        {
            State = VideoSessionState.Failed;
            return;
        }
        
        UpdateSessionData((VideoSessionDTO)sessionDTO);
    }

    public async Task<WebRTCNegotiationResult> Negotiate()
    {
        try
        {
            Peer.PeerConnection = InitPeerConnection();
            
            var sdp = await _coordinatorSession.RequestOffer(new WebRTCNegotiation(WebRTCNegotiationType.OfferRequest, Peer.PeerId, Id, null));

            if (sdp == null)
            {
                State = VideoSessionState.Failed;
                return WebRTCNegotiationResult.InternalError;
            }
            
            var result = Peer.PeerConnection.setRemoteDescription(sdp);
            if (result != SetDescriptionResultEnum.OK)
            {
                Peer.PeerConnection.Close("failed to set description");
                Peer.PeerConnection.Dispose();
                Peer.PeerConnection = null;
                State = VideoSessionState.Failed;
                return WebRTCNegotiationResult.InternalError;
            }

            var answer = Peer.PeerConnection.createAnswer();
            _coordinatorSession.SendAnswer(new WebRTCNegotiation(WebRTCNegotiationType.Answer, Peer.PeerId, Id, answer));
            
            State = VideoSessionState.ReadyForNegotiation;
            while (IceCandidatesBuffer.TryDequeue(out var iceCandidate))
            {
                Peer.PeerConnection.addIceCandidate(iceCandidate);
            }

            return WebRTCNegotiationResult.NoError;
        }
        catch (TimeoutException)
        {
            State = VideoSessionState.TimedOut;
            return WebRTCNegotiationResult.NotExceptedError; // TODO: Change to timed out
        }
    }
    
    public async Task<WebRTCNegotiationResult> Renegotiate()
    {
        try
        {
            Peer.PeerConnection = InitPeerConnection();
            
            var sdp = await _coordinatorSession.RequestOffer(new WebRTCNegotiation(WebRTCNegotiationType.Renegotiation, Peer.PeerId, Id, null));

            if (sdp == null)
            {
                State = VideoSessionState.Failed;
                return WebRTCNegotiationResult.InternalError;
            }
            
            var result = Peer.PeerConnection.setRemoteDescription(sdp);
            if (result != SetDescriptionResultEnum.OK)
            {
                Peer.PeerConnection.Close("failed to set description");
                Peer.PeerConnection.Dispose();
                Peer.PeerConnection = null;
                State = VideoSessionState.Failed;
                return WebRTCNegotiationResult.InternalError;
            }

            var answer = Peer.PeerConnection.createAnswer();
            _coordinatorSession.SendAnswer(new WebRTCNegotiation(WebRTCNegotiationType.Answer, Peer.PeerId, Id, answer));
            
            State = VideoSessionState.ReadyForNegotiation;
            while (IceCandidatesBuffer.TryDequeue(out var iceCandidate))
            {
                Peer.PeerConnection.addIceCandidate(iceCandidate);
            }

            return WebRTCNegotiationResult.NoError;
        }
        catch (TimeoutException)
        {
            State = VideoSessionState.TimedOut;
            return WebRTCNegotiationResult.NotExceptedError; // TODO: Change to timed out
        }
    }

    private RTCPeerConnection InitPeerConnection()
    {
        RTCPeerConnection pc = new RTCPeerConnection();
        AddTracks(pc);
        
        pc.onicecandidate += (iceCandidate) =>
        {
            if (pc.signalingState == RTCSignalingState.have_remote_offer)
            {
                var candidateDTO = new RTCIceCandidateInit()
                {
                    candidate = iceCandidate.candidate,
                    sdpMid = iceCandidate.sdpMid,
                    sdpMLineIndex = iceCandidate.sdpMLineIndex
                };
                _coordinatorSession.SendICE(new WebRTCNegotiation(WebRTCNegotiationType.ICE, Peer.PeerId, Id, candidateDTO));
            }
        };
        
        if (HostId != _coordinatorSession.GetUser().Id)
        {
            pc.OnVideoFrameReceived += _ffmpegVideoEndPoint.GotVideoFrame;
        }
        pc.OnVideoFormatsNegotiated += (formats) =>
        {
            _videoFormat = formats.First();
            _ffmpegVideoEndPoint.SetVideoSourceFormat(formats.First());
        };
        
        pc.onconnectionstatechange += (state) =>
        {
            if (state == RTCPeerConnectionState.connected)
            {
                State = VideoSessionState.Connected;
                StartReceive();
            }
            else if (state == RTCPeerConnectionState.failed)
            {
                State = VideoSessionState.Failed;
                EndReceive();
            }
            else if (state == RTCPeerConnectionState.closed)
            {
                State = VideoSessionState.Ended;
                EndReceive();
            }
            else if (state == RTCPeerConnectionState.disconnected)
            {
                State = VideoSessionState.Disconnected;
                EndReceive();
            }
        };
        return pc;
    }

    private void AddTracks(RTCPeerConnection pc)
    {
        if (HostId == _coordinatorSession.GetUser().Id)
        {
            Peer._videoTrack = new MediaStreamTrack(
                new VideoFormat(96, "H264", 90000, "profile-level-id=42e01f;packetization-mode=1;")
            );
            pc.addTrack(Peer._videoTrack);
            _ffmpegVideoEndPoint.RestrictFormats(format => format.Codec == VideoCodecsEnum.H264);
            if (IsAudioRequested)
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

                pc.addTrack(new MediaStreamTrack(audioFormats));
            }
        }
        else
        {
            Peer._videoTrack = new MediaStreamTrack(
                new VideoFormat(96, "H264", 90000, "profile-level-id=42e01f;packetization-mode=1;")
            );
            pc.addTrack(Peer._videoTrack);
            _ffmpegVideoEndPoint.RestrictFormats(format => format.Codec == VideoCodecsEnum.H264);
            if (IsAudioRequested)
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

                pc.addTrack(new MediaStreamTrack(audioFormats));
            }
        }
    }
    
    public void StartReceive()
    {
        if (State == VideoSessionState.Connected)
        {
            _ffmpegVideoEndPoint.OnVideoSinkDecodedSampleFaster += OnVideoFrameReceived;
            _ffmpegVideoEndPoint.StartVideo();
            if (IsAudioRequested)
            {
                Peer.PeerConnection!.OnAudioFrameReceived += OnAudioFrameReceived;
                _audioPlayerService!.StartAsync();
            }
        }
    }

    public void EndReceive()
    {
        if (State is VideoSessionState.Disconnected or VideoSessionState.Ended)
        {
            _ffmpegVideoEndPoint.OnVideoSinkDecodedSampleFaster -= OnVideoFrameReceived;
            _ffmpegVideoEndPoint.CloseVideo();
            if (IsAudioRequested)
            {
                Peer.PeerConnection!.OnAudioFrameReceived -= OnAudioFrameReceived;
                _audioPlayerService!.Stop();
            }
        }
    }
    
    public async Task<bool> StartSending()
    {
        Console.WriteLine("Initializing...");
        await _screenCastClient.InitializeAsync();
        if (!await _screenCastClient.CreateSessionAsync())
        {
            await _screenCastClient.CreateSessionAsync();
        }
        await _screenCastClient.SelectSourcesAsync();
        if (await _screenCastClient.StartSessionAsync())
        {
            State = VideoSessionState.WaitingForNegotiation;
            _ = Negotiate();
            _screenCastClient.FrameReceived += OnVideoFrameReady;
            return true;
        }
        State = VideoSessionState.Canceled;
        return false;
    }
    
    public async Task StopSending()
    {
        if (await _screenCastClient.CloseSessionAsync())
        {
            _screenCastClient.FrameReceived -= OnVideoFrameReady;
        }
    }
    
    private void OnVideoFrameReceived(RawImage rawImage)
    {
        FrameReceived?.Invoke(_frameProcessor.ProceedRawFrame(rawImage));
        var elapsedMs = stopwatch.ElapsedMilliseconds;
        Console.WriteLine($"Frame interval {elapsedMs} мс");
        stopwatch.Reset();
    }

    private void OnVideoFrameReady(byte[] encodedFrame)
    {
        stopwatch.Start();
        uint clockRate = 90000;
        double frameDurationSeconds = 1.0 / 60.0;
        uint durationRtpUnits = (uint)(clockRate * frameDurationSeconds);
        _ffmpegVideoEndPoint.GotVideoFrame(IPEndPoint.Parse("1.1.1.1"), durationRtpUnits, encodedFrame, 
            new VideoFormat(96, "H264", 90000, "profile-level-id=42e01f;packetization-mode=1;"));
        Peer.PeerConnection?.SendVideo(durationRtpUnits, encodedFrame);
    }

    private void OnAudioFrameReceived(EncodedAudioFrame frame)
    {
        _audioPlayerService!.PlayPcm(frame.EncodedAudio);
    }
    
    public void Dispose()
    {
        State = VideoSessionState.Ended;
        _ffmpegVideoEndPoint.CloseVideo();
        Peer.PeerConnection?.Close("normal");
        _screenCastClient.Dispose(); 
        _audioPlayerService?.Dispose();
        _frameProcessor?.Dispose();
        Peer.PeerConnection?.Dispose();
        _ffmpegVideoEndPoint.CloseVideo();
        _ffmpegVideoEndPoint.Dispose();
        _ffmpegVideoEndPoint?.Dispose();
        _audioPlayerService?.Dispose();
        _screenCastClient?.Dispose();
        _frameProcessor?.Dispose(); 
        GC.SuppressFinalize(this);
    }
}