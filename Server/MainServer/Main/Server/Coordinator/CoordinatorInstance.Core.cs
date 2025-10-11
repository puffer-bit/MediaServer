using Server.MainServer.Main.Server.Coordinator.Connection;
using Server.MainServer.Main.Server.Coordinator.Connection.Manager;
using Server.MainServer.Main.Server.Coordinator.Manager;
using Server.MainServer.Main.Server.Coordinator.MessagesProcessing.MessageSender.Sender;
using Server.MainServer.Main.Server.Coordinator.MessagesProcessing.MessagesHandlers;
using Server.MainServer.Main.Server.Coordinator.Users.Manager;
using Server.MainServer.Main.Server.Coordinator.WebRTC.Manager;
using Server.MainServer.Main.Server.Factories.CoordinatorFactory;
using Shared.Enums;
using Shared.Models;
using SIPSorcery.Media;
using SIPSorcery.Net;
using SIPSorceryMedia.Abstractions;
using SIPSorceryMedia.FFmpeg;

//using SFU_MainCluster.SFU.Main.Server.Coordinator.MessagesProcessing.MessagesValidator;

namespace Server.MainServer.Main.Server.Coordinator
{
    public partial class CoordinatorInstance
    {
        private readonly ISessionManager _sessionManager;
        private readonly IConnectionManager _connectionManager;
        private readonly IUserManager _userManager;
        private readonly IMessageSender _messageSender;
        private readonly IWebRTCManager _webRTCManager;
        private readonly IHeartbeatManager _heartbeatManager;
        private readonly ILogger _logger;
        private readonly Dictionary<MessageType, IMessageHandler> _handlers = new();
        
        public CoordinatorInstance( 
            ILoggerFactory loggerFactory,
            ICoordinatorFactory coordinatorFactory)
        {
            coordinatorFactory.Initialize(this);
            _webRTCManager = coordinatorFactory.CreateWebRTCManager();
            _messageSender = coordinatorFactory.CreateMessageSender();
            _userManager = coordinatorFactory.CreateUserManager();
            _connectionManager = coordinatorFactory.CreateConnectionManager();
            _sessionManager = coordinatorFactory.CreateSessionManager();
            _heartbeatManager = coordinatorFactory.CreateHeartbeatManager();
            _logger = loggerFactory.CreateLogger("Coordinator");
            //_validator = validator;
            foreach (var handler in coordinatorFactory.CreateMessageHandlers())
            {
                _handlers[handler.Type] = handler;
            }
            _userManager.AddUser(new UserDTO()
            {
                Id = "system",
                Name = "system",
                State = UserState.Normal
            }, out _);
            
            _logger.LogInformation("Server started.");
        }
        
        [Obsolete]
        public void RegisterHandler(MessageType type, IMessageHandler handler)
        {
            _handlers[type] = handler;
        }

        public async Task RegisterTestSessions()
        {
            await CreateTestVideoSessionFromMp4();
            await CreateTestVideoSessionFromPattern();
            _logger.LogWarning("Beware of continuous use of test sessions. This triggers video encoding and increases the load on the hardware many times over.");
        }
        
        private async Task CreateTestVideoSessionFromMp4()
        {
            _sessionManager.CreateVideoSession(new VideoSessionDTO()
            {
                Capacity = 4,
                HostId = "system",
                SessionType = SessionType.Video,
                Id = "9873121",
                Name = "Test1",
                IsAudioRequested = true
            }, "system");
            
            _sessionManager.GetVideoSession("9873121", out var videoSession);
            _sessionManager.JoinVideoSession(videoSession.AsModel(), "system");
            videoSession.GetPeerByUserId("system", out var peer);
            
            var pc = _webRTCManager.CreatePeerConnection("system","9873121", peer.GetId(), true, true, true);
            var mediaFileSource = new SIPSorceryMedia.FFmpeg.FFmpegFileSource("Resources/Media/test_video1.mp4", true, null);
            mediaFileSource.RestrictFormats(x => x.Codec == VideoCodecsEnum.H264);
            var audioSource = new AudioExtrasSource(new AudioEncoder(includeOpus: false), new AudioSourceOptions { AudioSource = AudioSourcesEnum.Music, MusicFile = "Resources/Media/test_audio1.pcm", MusicInputSamplingRate = AudioSamplingRatesEnum.Rate44_1kHz});
            audioSource.OnAudioSourceEncodedSample += pc.SendAudio;
            pc.OnAudioFormatsNegotiated += (audioFormats) =>
            {
                audioSource.SetAudioSourceFormat(audioFormats.First());
            };
            mediaFileSource.OnVideoSourceEncodedSample += (durationRtpUnits, sample) =>
            {
                pc.SendVideo(durationRtpUnits, sample);
            };
            pc.OnVideoFormatsNegotiated += (formats) =>
            {
                mediaFileSource.SetVideoSourceFormat(formats.First());
            };
            pc.onicecandidate += (candidate) =>
            {
                _webRTCManager.ReactOnICE(peer.GetId(), candidate);
            };
            pc.onconnectionstatechange += async (state) =>
            {
                if (state == RTCPeerConnectionState.connected)
                {
                    await mediaFileSource.StartVideo();
                    await audioSource.StartAudio();
                    //_logger.LogInformation("TestSession started.");
                }
                else if (state == RTCPeerConnectionState.failed)
                {
                    //_logger.LogError("TestSession failed. (PeerConnection State is {state})", state.ToString());
                }
            };
            
            var result = pc.setRemoteDescription(await _webRTCManager.ReactOnOfferRequest(peer.GetId()));
            if (result == SetDescriptionResultEnum.OK)
            {
                _webRTCManager.ReactOnAnswer(peer.GetId(), pc.createAnswer());
            }
            else
            {
                _logger.LogError("TestSession sdp failed.");
            }
        }
        
        private async Task CreateTestVideoSessionFromPattern()
        {
            _sessionManager.CreateVideoSession(new VideoSessionDTO()
            {
                Capacity = 4,
                HostId = "system",
                SessionType = SessionType.Video,
                Id = "987312",
                Name = "Test2",
                IsAudioRequested = true
            }, "system");
            
            _sessionManager.GetVideoSession("987312", out var videoSession);
            _sessionManager.JoinVideoSession(videoSession.AsModel(), "system");
            videoSession.GetPeerByUserId("system", out var peer);
            
            var pc = _webRTCManager.CreatePeerConnection("system","987312", peer.GetId(), true, true, true);
            
            var testPatternSource = new VideoTestPatternSource(new FFmpegVideoEncoder());
            testPatternSource.OnVideoSourceEncodedSample += pc.SendVideo;
            pc.OnVideoFormatsNegotiated += (formats) => testPatternSource.SetVideoSourceFormat(formats.First());
            var audioSource = new AudioExtrasSource(new AudioEncoder(includeOpus: false), new AudioSourceOptions { AudioSource = AudioSourcesEnum.Music, MusicFile = "Resources/Media/test_audio2.pcm", MusicInputSamplingRate = AudioSamplingRatesEnum.Rate44_1kHz});
            //var audioSource = new AudioExtrasSource(new AudioEncoder(includeOpus: false), new AudioSourceOptions { AudioSource = AudioSourcesEnum.SineWave});
            audioSource.OnAudioSourceEncodedSample += pc.SendAudio;
            pc.OnAudioFormatsNegotiated += (audioFormats) =>
            {
                audioSource.SetAudioSourceFormat(audioFormats.First());
            };
            pc.onicecandidate += (candidate) =>
            {
                _webRTCManager.ReactOnICE(peer.GetId(), candidate);
            };
            pc.onconnectionstatechange += async (state) =>
            {
                if (state == RTCPeerConnectionState.connected)
                {
                    await testPatternSource.StartVideo();
                    await audioSource.StartAudio();
                    //_logger.LogInformation("TestSession started.");
                }
                else if (state == RTCPeerConnectionState.failed)
                {
                    //_logger.LogError("TestSession failed. (PeerConnection State is {state})", state.ToString());
                }
            };

            var sdp = await _webRTCManager.ReactOnOfferRequest(peer.GetId());
            var result = pc.setRemoteDescription(sdp);
            if (result == SetDescriptionResultEnum.OK)
            {
                _webRTCManager.ReactOnAnswer(peer.GetId(), pc.createAnswer());
            }
            else
            {
                _logger.LogError("TestSession sdp failed.");
            }
        }
        
        public void ProcessEvent(BaseMessage message)
        {
            if (message.UserId == null)
            {
                return;
            }

            if (_handlers.TryGetValue(message.Type, out var handler))
            {
                handler.HandleMessage(message);
                return;
            }
            
            switch (message.Type)
            {
                case MessageType.Undefined:
                    _logger.LogWarning("Undefined event received, something went wrong.");
                    
                    break;

                default:
                    _logger.LogError("Cant read event data.");
                    break;

            }
        }
    }
}
