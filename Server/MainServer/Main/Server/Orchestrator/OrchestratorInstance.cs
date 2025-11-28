using System.Collections.Concurrent;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Fleck;
using Server.Domain.Entities;
using Server.Domain.Enums;
using Server.Domain.Repositories.CoordinatorInstanceRepository;
using Server.Domain.Repositories.ServerInstanceRepository;
using Server.Domain.Repositories.VideoSessionRepository;
using Server.MainServer.Main.Server.Bootstrap;
using Server.MainServer.Main.Server.Coordinator;
using Server.MainServer.Main.Server.Factories.CoordinatorFactory;
using Server.MainServer.Main.Server.Orchestrator.WebSocket;
using Shared.Models;

namespace Server.MainServer.Main.Server.Orchestrator
{
    public class OrchestratorInstance : IOrchestratorInstance
    {
        public required IOrchestratorInstanceContext Context { get; init; }
        public required ConcurrentDictionary<string, ICoordinatorInstance> CoordinatorsPool { get; init; } = new();
        public bool IsGatewayReady { get; set; }
        public bool IsReady { get; set; }
        public bool IsStarted { get; set; }
        public bool IsFfmpegEnabled { get; set; }
        public bool IsFfmpegInitialized { get; set; }
        public bool IsHardwareAccelerated { get; set; }
        public int ActiveSessionsCount { get; set; }
        public int ConnectedUsersCount { get; set; }
        public int RegisteredUsersCount { get; set; }
        public double CurrentCpuLoadPercent { get; set; }
        public double CurrentMemoryConsumption { get; set; }
        public int RamSizeMb { get; set; }
        public int MaxDbSizeMb { get; set; }
        public bool IsRemoteConnectionsAvailable { get; set; }
        public bool IsRemoteConnectionsRestricted { get; set; }
        public bool IsTestSessionsAllowed { get; set; }
        public ServerState State { get; set; }

        private readonly ILogger _logger;

        private WebSocketServer? _webSocketServer;
        
        private readonly IServiceProvider _serviceProvider;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ICoordinatorInstanceFactory _coordinatorInstanceFactory;
        private readonly InitialServerLoader _initialServerLoader;
        public event EventHandler<ServerStateChangedEventArgs>? StateChanged;
        
        public OrchestratorInstance(IOrchestratorInstanceContext orchestratorInstanceContext,
            ILoggerFactory loggerFactory,
            ICoordinatorInstanceFactory coordinatorInstanceFactory, 
            IServiceScopeFactory scopeFactory,
            IServiceProvider serviceProvider,
            InitialServerLoader initialServerLoader)
        {
            State = ServerState.Starting;
            Context = orchestratorInstanceContext;
            _initialServerLoader = initialServerLoader;
            _logger = loggerFactory.CreateLogger($"Orchestrator");
            
            _coordinatorInstanceFactory = coordinatorInstanceFactory;
            _scopeFactory = scopeFactory;
            _serviceProvider = serviceProvider;
        }

        public async Task Configure()
        {
            _logger.LogTrace("Configuring orchestrator...");
            IsFfmpegEnabled = _initialServerLoader.Context.MainServer.EnableFfmpeg;
            IsFfmpegInitialized = _initialServerLoader.IsFfmpegInitialized;

            using var scope = _scopeFactory.CreateScope();
            var orchestratorInstanceRepository = scope.ServiceProvider.GetRequiredService<IOrchestratorInstanceRepository>();
            var orchestratorConfig = await orchestratorInstanceRepository.GetAsync();
            if (orchestratorConfig == null)
            {
                _logger.LogTrace("Database not contains config. Creating...");
                
                Context.CreateContext();
                await orchestratorInstanceRepository.SaveOrUpdateAsync(Context.GetAsEntity());
                
                await AttachCoordinatorInstance(await CreateCoordinatorInstanceAsync("Default server", "127.0.0.1", 26666));
            }
            else
            {
                Context.UpdateContext(orchestratorConfig);
            }
        }
        
        public async Task StartAsync(CancellationToken ct = default)
        {
            _logger.LogInformation("Starting orchestrator...");

            try
            {
                CreateAndStartWebSocketServer(_serviceProvider.GetRequiredService<ILoggerFactory>());
            
                await FetchCoordinatorsInstancesFromDbAsync();
            }
            catch (CryptographicException e)
            {
                _logger.LogCritical("Orchestrator failed. Failed to found certificate for WebSocket connection.");
                await StopAsync(CancellationToken.None);
                Environment.Exit(1);
            }
            catch (Exception e)
            {
                _logger.LogCritical("Orchestrator failed. Exception: {e}", e.Message);
                await StopAsync(CancellationToken.None);
                Environment.Exit(1);
            }
            
            State = ServerState.Online;
            _logger.LogInformation("Orchestrator successfully started.");
        }

        public async Task StopAsync(CancellationToken ct = default)
        {
            _logger.LogInformation("Closing orchestrator...");

            foreach (var coordinatorInstance in CoordinatorsPool)
            {
                coordinatorInstance.Value.CloseCoordinator();
            }
            State = ServerState.Offline;
        }

        public Task RestartAsync(CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task EnterMaintenanceModeAsync()
        {
            throw new NotImplementedException();
        }

        public Task CheckDatabaseStateAsync()
        {
            throw new NotImplementedException();
        }

        public Task MigrateToNewDatabaseAsync(string targetConnectionString)
        {
            throw new NotImplementedException();
        }

        public Task BackupDatabaseAsync(string backupPath)
        {
            throw new NotImplementedException();
        }

        public Task RestoreDatabaseBackupAsync(string backupFilePath)
        {
            throw new NotImplementedException();
        }

        public Task BackupMediaAsync(string targetPath)
        {
            throw new NotImplementedException();
        }

        public Task RestoreMediaBackupAsync(string sourcePath)
        {
            throw new NotImplementedException();
        }

        public Task CollectRegionDataAsync()
        {
            throw new NotImplementedException();
        }

        public Task CollectHostSystemDataAsync()
        {
            throw new NotImplementedException();
        }

        public void SetLaunchTime(DateTime utcTime)
        {
            throw new NotImplementedException();
        }

        public async Task<CoordinatorInstanceEntity> CreateCoordinatorInstanceAsync(string name, string ip, int port)
        {
            using var scope = _scopeFactory.CreateScope();
            var coordinatorsInstancesRepository = scope.ServiceProvider.GetRequiredService<ICoordinatorInstanceRepository>();

            var coordinatorInstanceEntity = new CoordinatorInstanceEntity()
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                Ip = ip,
                Port = port,
                CreateTime = DateTime.UtcNow,
            };
            await coordinatorsInstancesRepository.AddAsync(coordinatorInstanceEntity);
            
            return coordinatorInstanceEntity;
        }
        
        public async Task AttachCoordinatorInstance(CoordinatorInstanceEntity coordinatorInstanceEntity)
        {
            // TODO: Remove after full orchestrator integration
            coordinatorInstanceEntity.Ip = _initialServerLoader.Context.MainServer.IpAddress;
            coordinatorInstanceEntity.Port = _initialServerLoader.Context.MainServer.Port;
            
            coordinatorInstanceEntity.IsTurnEnabled = _initialServerLoader.Context.MainServer.EnableTurn;
            coordinatorInstanceEntity.TurnAddress = _initialServerLoader.Context.MainServer.TurnAddress;
            coordinatorInstanceEntity.TurnPort = _initialServerLoader.Context.MainServer.TurnPort;
            coordinatorInstanceEntity.TurnUsername = _initialServerLoader.Context.MainServer.TurnUsername;
            coordinatorInstanceEntity.TurnPassword = _initialServerLoader.Context.MainServer.TurnPassword;
            
            coordinatorInstanceEntity.IsStunEnabled = _initialServerLoader.Context.MainServer.EnableStun;
            coordinatorInstanceEntity.StunAddress = _initialServerLoader.Context.MainServer.StunAddress;
            coordinatorInstanceEntity.StunPort = _initialServerLoader.Context.MainServer.StunPort;
            
            using var scope = _scopeFactory.CreateScope();
            var coordinatorInstanceRepository = scope.ServiceProvider.GetRequiredService<ICoordinatorInstanceRepository>();
            await coordinatorInstanceRepository.UpdateAsync(coordinatorInstanceEntity);
            //
            
            CoordinatorsPool.TryAdd(coordinatorInstanceEntity.Id, _coordinatorInstanceFactory.CreateCoordinatorInstance(Context.ServerVersion, coordinatorInstanceEntity));

            CoordinatorsPool[coordinatorInstanceEntity.Id].VideoSessionAdded += AddVideoSessionToDbAsync;
            CoordinatorsPool[coordinatorInstanceEntity.Id].VideoSessionReconfigured += UpdateVideoSessionToDbAsync;
            CoordinatorsPool[coordinatorInstanceEntity.Id].VideoSessionRemoved += RemoveVideoSessionFromDbAsync;
                
            var videoSessionRepository = scope.ServiceProvider.GetRequiredService<IVideoSessionRepository>();
            
            // Config coordinator
            await CoordinatorsPool[coordinatorInstanceEntity.Id].Configure(_serviceProvider.GetRequiredService<ILoggerFactory>(), 
                coordinatorInstanceEntity);
            
            await CoordinatorsPool[coordinatorInstanceEntity.Id].LoadSessions(await videoSessionRepository.GetByCoordinatorIdAsync(coordinatorInstanceEntity.Id));
        }

        public void DetachCoordinatorInstance(string coordinatorInstanceId)
        {
            if (CoordinatorsPool.TryRemove(coordinatorInstanceId, out var coordinatorInstance))
                coordinatorInstance.CloseCoordinator();
        }

        public async Task DisableCoordinatorInstanceAsync(string coordinatorInstanceId)
        {
            throw new NotImplementedException();
        }

        public async Task EnableCoordinatorInstanceAsync(string coordinatorInstanceId)
        {
            throw new NotImplementedException();
        }

        public async Task CommitCoordinatorDataAsync()
        {
            throw new NotImplementedException();
        }

        public async Task RemoveCoordinatorInstanceAsync(string coordinatorInstanceId)
        {
            throw new NotImplementedException();
        }

        public async Task RestoreCoordinatorInstanceAsync(string coordinatorInstanceId)
        {
            throw new NotImplementedException();
        }

        public async Task PurgeCoordinatorInstanceAsync(string coordinatorInstanceId)
        {
            throw new NotImplementedException();
        }

        public void EnableRemoteConnections()
        {
            throw new NotImplementedException();
        }

        public void RestrictRemoteConnections(bool applyToActiveConnections)
        {
            throw new NotImplementedException();
        }

        public void DisableRemoteConnections()
        {
            throw new NotImplementedException();
        }

        public async Task AddAllowedIpAsync(string ip)
        {
            throw new NotImplementedException();
        }

        public async Task ProcessExternalAction()
        {
            throw new NotImplementedException();
        }
        
        private void MigrateDb()
        {
            
        }
        
        public void CreatePfxCert()
        {

        }

        private void CreateAndStartWebSocketServer(ILoggerFactory loggerFactory)
        {
            var cert = X509CertificateLoader.LoadPkcs12FromFile("Certs/server.pfx", "MyPassword");
            
            if (string.IsNullOrWhiteSpace(Context.Ip) || Context.Port == 0)
            {
                _webSocketServer = new WebSocketServer("wss://127.0.0.1:26666/");
            }
            else if (IPAddress.TryParse(Context.Ip, out _))
            {
                _webSocketServer = new WebSocketServer($"wss://{Context.Ip}:{Context.Port}/");
            }
            else
            {
                _logger.LogWarning("Failed to parse IP address.");
                _webSocketServer = new WebSocketServer("wss://127.0.0.1:26666/");
            }
            
            _webSocketServer.EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
            _webSocketServer.Certificate = cert;

            _webSocketServer.Start(socket =>
            {
                var handler = new OrchestratorInstanceWebSocketGateway(this, socket, loggerFactory);

                socket.OnOpen = handler.OnOpen;
                socket.OnClose = handler.OnClose;
                socket.OnMessage = handler.OnMessage;
                socket.OnError = handler.OnError;
            });

            IsGatewayReady = true;
            _logger.LogInformation("Orchestrator WebSocket server listening on IP:{Ip} and Port:{Port}.", Context.Ip, Context.Port);
        }
        
        private async Task FetchCoordinatorsInstancesFromDbAsync()
        {
            _logger.LogTrace("Fetching coordinators from database...");

            using var scope = _scopeFactory.CreateScope();
            var coordinatorsInstancesRepository = scope.ServiceProvider.GetRequiredService<ICoordinatorInstanceRepository>();
            
            foreach (var coordinatorInstanceEntity in await coordinatorsInstancesRepository.GetAllAsync())
            {
                // Adding fetched coordinator to pool
                await AttachCoordinatorInstance(coordinatorInstanceEntity);
            }
        }

        private async Task AddVideoSessionToDbAsync(VideoSessionDTO videoSessionDTO)
        {
            using var scope = _scopeFactory.CreateScope();
            var videoSessionRepository = scope.ServiceProvider.GetRequiredService<IVideoSessionRepository>();

            await videoSessionRepository.AddAsync(new VideoSessionEntity()
            {
                CoordinatorInstanceId = videoSessionDTO.CoordinatorInstanceId,
                Name = videoSessionDTO.Name,
                Id = videoSessionDTO.Id,
                Capacity = videoSessionDTO.Capacity,
                HostId = videoSessionDTO.HostId,
                CreatedAt = DateTime.UtcNow
            });
        }
        
        private async Task UpdateVideoSessionToDbAsync(VideoSessionDTO videoSessionDTO)
        {
            using var scope = _scopeFactory.CreateScope();
            var videoSessionRepository = scope.ServiceProvider.GetRequiredService<IVideoSessionRepository>();

            await videoSessionRepository.UpdateAsync(new VideoSessionEntity()
            {
                CoordinatorInstanceId = videoSessionDTO.CoordinatorInstanceId,
                Name = videoSessionDTO.Name,
                Id = videoSessionDTO.Id,
                Capacity = videoSessionDTO.Capacity,
                HostId = videoSessionDTO.HostId,
                CreatedAt = DateTime.UtcNow
            });
        }

        private async Task RemoveVideoSessionFromDbAsync(string videoSessionId)
        {
            using var scope = _scopeFactory.CreateScope();
            var videoSessionRepository = scope.ServiceProvider.GetRequiredService<IVideoSessionRepository>();
            
            await videoSessionRepository.RemoveAsync(videoSessionId);
        }
    }
}
