using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Fleck;
using Server.Domain.Entities;
using Server.Domain.Enums;
using Server.MainServer.Main.Server.Coordinator.Connection.ConnectionManager;
using Server.MainServer.Main.Server.Coordinator.Connection.HeartbeatManager;
using Server.MainServer.Main.Server.Coordinator.Messages.MessageSender.Sender;
using Server.MainServer.Main.Server.Coordinator.Messages.MessagesHandlers;
using Server.MainServer.Main.Server.Coordinator.Sessions.Manager;
using Server.MainServer.Main.Server.Coordinator.Users.Authenticator;
using Server.MainServer.Main.Server.Coordinator.Users.Manager;
using Server.MainServer.Main.Server.Coordinator.WebRTC.Manager;
using Server.MainServer.Main.Server.Coordinator.WebSocket;
using Server.MainServer.Main.Server.Factories.CoordinatorFactory;
using Shared.Enums;
using Shared.Models;
using Shared.Models.DTO;
using SIPSorcery.Media;
using SIPSorcery.Net;
using SIPSorceryMedia.Abstractions;
using SIPSorceryMedia.FFmpeg;

//using SFU_MainCluster.SFU.Main.Server.Coordinator.MessagesProcessing.MessagesValidator;

namespace Server.MainServer.Main.Server.Coordinator
{
    public partial class CoordinatorInstance : ICoordinatorInstance
    {
        public ICoordinatorInstanceContext Context { get; init; }
        public bool IsReady { get; set; }
        public bool IsGatewayReady { get; set; }
        public bool IsStarted { get; set; }
        public bool IsRemoteConnectionsAvailable { get; set; }
        public bool IsRemoteConnectionsRestricted { get; set; }
        public CoordinatorState State { get; set; }

        private WebSocketServer? _webSocketServer;
        
        public event Func<string, Task>? ContextChanged;
        public event Func<string, Task>? RequestClose;
        public event Func<VideoSessionDTO, Task>? VideoSessionAdded;
        public event Func<VideoSessionDTO, Task>? VideoSessionReconfigured;
        public event Func<string, Task>? VideoSessionRemoved;

        private readonly ILogger _logger;
        
        private readonly ISessionManager _sessionManager;
        private readonly IConnectionManager _connectionManager;
        private readonly IAuthenticator _authenticator;
        private readonly IUserManager _userManager;
        private readonly IMessageSender _messageSender;
        private readonly IWebRTCManager _webRTCManager;
        private readonly IHeartbeatManager _heartbeatManager;
        private readonly Dictionary<MessageType, IMessageHandler> _handlers = new();
        
        public CoordinatorInstance(CoordinatorInstanceEntity coordinatorInstanceEntity,
            ILoggerFactory loggerFactory,
            ICoordinatorInstanceFactory coordinatorInstanceFactory)
        {
            State = CoordinatorState.Starting;
            Context = coordinatorInstanceFactory.CreateCoordinatorInstanceContext(coordinatorInstanceEntity.Id);
            Context.LoadContext(coordinatorInstanceEntity);
            
            _logger = loggerFactory.CreateLogger($"Coordinator {Context.Name}");
            
            _webRTCManager = coordinatorInstanceFactory.CreateWebRTCManager(this);
            _messageSender = coordinatorInstanceFactory.CreateMessageSender(this);
            _authenticator = coordinatorInstanceFactory.CreateAuthenticator(this);
            _userManager = coordinatorInstanceFactory.CreateUserManager(this);
            _connectionManager = coordinatorInstanceFactory.CreateConnectionManager(this);
            _sessionManager = coordinatorInstanceFactory.CreateSessionManager(this);
            _heartbeatManager = coordinatorInstanceFactory.CreateHeartbeatManager(this);
            //_validator = validator;
            foreach (var handler in coordinatorInstanceFactory.CreateMessageHandlers(this))
            {
                _handlers[handler.Type] = handler;
            }
        }
        
        public async Task RegisterTestSessions()
        {
            await CreateTestVideoSessionFromMp4();
            await CreateTestVideoSessionFromPattern();
            _logger.LogWarning("Beware of continuous use of test sessions. " +
                               "This triggers video encoding and increases the load on the hardware many times over.");
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
        
        public void CloseCoordinator()
        {
            IsStarted = false;
            State = CoordinatorState.Closing;
            //_logger.LogInformation("Coordinator {Id} closing...", Context.Id);
            _webRTCManager.Dispose();
            _sessionManager.CloseAllSessions();
            _connectionManager.DisconnectAllUsers(false);
            _connectionManager.Dispose();
            _logger.LogInformation("Coordinator {Id} closed.", Context.Id);
        }

        private void CreateWebSocketServer(ILoggerFactory loggerFactory)
        {
            var cert = X509CertificateLoader.LoadPkcs12FromFile("Certs/server.pfx", "MyPassword");
            _webSocketServer = new WebSocketServer($"wss://{Context.Ip}:{Context.Port}/");
            _webSocketServer.EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
            _webSocketServer.Certificate = cert;

            _webSocketServer.Start(socket =>
            {
                var handler = new CoordinatorInstanceWebSocketGateway(this, socket, loggerFactory);

                socket.OnOpen = handler.OnOpen;
                socket.OnClose = handler.OnClose;
                socket.OnMessage = handler.OnMessage;
                socket.OnError = handler.OnError;
            });

            IsGatewayReady = true;
            _logger.LogInformation("Coordinator {Id} WebSocket server listening on IP:{Ip} and Port:{Port}.", Context.Id, Context.Ip, Context.Port);
        }
        
        private async Task CreateTestVideoSessionFromMp4()
        {
            _sessionManager.CreateVideoSession(new VideoSessionDTO()
            {
                CoordinatorInstanceId = Context.Id,
                Capacity = 4,
                HostId = "system",
                SessionType = SessionType.Video,
                Id = "9873121",
                Name = "Test1",
                IsAudioRequested = true
            }, "system");
            
            _sessionManager.GetVideoSession("9873121", out var videoSession);
            _sessionManager.JoinVideoSession(videoSession!.AsModel(), "system");
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
                CoordinatorInstanceId = Context.Id,
                Capacity = 4,
                HostId = "system",
                SessionType = SessionType.Video,
                Id = "987312",
                Name = "Test2",
                IsAudioRequested = true
            }, "system");
            
            _sessionManager.GetVideoSession("987312", out var videoSession);
            _sessionManager.JoinVideoSession(videoSession!.AsModel(), "system");
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
    }
}
