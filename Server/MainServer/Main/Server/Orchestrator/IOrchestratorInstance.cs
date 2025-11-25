using System.Collections.Concurrent;
using Server.Domain.Entities;
using Server.Domain.Enums;
using Server.MainServer.Main.Server.Bootstrap;
using Server.MainServer.Main.Server.Coordinator;

namespace Server.MainServer.Main.Server.Orchestrator
{
    public interface IOrchestratorInstance
    {
        IOrchestratorInstanceContext Context { get; init; }
        ConcurrentDictionary<string, ICoordinatorInstance> CoordinatorsPool { get; init; }
        bool IsGatewayReady { get; set; }
        bool IsReady { get; set; }
        bool IsStarted { get; set; }
        bool IsFfmpegEnabled { get; set; }
        bool IsFfmpegInitialized { get; set; }
        TimeSpan UpTime => DateTime.UtcNow - Context.CurrentLaunchTime;
        bool IsHardwareAccelerated { get; set; }
        int ActiveSessionsCount { get; set; }
        int ConnectedUsersCount { get; set; }
        int RegisteredUsersCount { get; set; }
        double CurrentCpuLoadPercent { get; set; }
        double CurrentMemoryConsumption { get; set; }
        int RamSizeMb { get; set; }
        int MaxDbSizeMb { get; set; }
        
        Task Configure(InitialServerLoader initialServerLoader);
        
        // Lifecycle actions
        Task StartAsync(CancellationToken ct = default);
        Task StopAsync(CancellationToken ct = default);
        Task RestartAsync(CancellationToken ct = default);
        Task EnterMaintenanceModeAsync();

        // DB Maintenance
        Task CheckDatabaseStateAsync();
        Task MigrateToNewDatabaseAsync(string targetConnectionString);
        Task BackupDatabaseAsync(string backupPath);
        Task RestoreDatabaseBackupAsync(string backupFilePath);

        // Media Maintenance
        Task BackupMediaAsync(string targetPath);
        Task RestoreMediaBackupAsync(string sourcePath);

        // Server init
        Task CollectRegionDataAsync();
        Task CollectHostSystemDataAsync();
        void SetLaunchTime(DateTime utcTime);

        // Health and statistics
        //Task<ServerHealthStatus> GetHealthStatusAsync();
        //Task<ServerMetrics> GetCurrentMetricsAsync();

        // CoordinatorInstances actions
        Task<CoordinatorInstanceEntity> CreateCoordinatorInstanceAsync(string name, string ip, int port);
        Task AttachCoordinatorInstance(CoordinatorInstanceEntity coordinatorInstanceEntity);
        void DetachCoordinatorInstance(string coordinatorInstanceId);
        Task DisableCoordinatorInstanceAsync(string coordinatorInstanceId);
        Task EnableCoordinatorInstanceAsync(string coordinatorInstanceId);
        Task CommitCoordinatorDataAsync();
        Task RemoveCoordinatorInstanceAsync(string coordinatorInstanceId);
        Task RestoreCoordinatorInstanceAsync(string coordinatorInstanceId);
        Task PurgeCoordinatorInstanceAsync(string coordinatorInstanceId);

        // Connection actions
        void EnableRemoteConnections();
        void RestrictRemoteConnections(bool applyToActiveConnections);
        void DisableRemoteConnections();
        Task AddAllowedIpAsync(string ip);

        // Other
        Task ProcessExternalAction();

        // Events
        event EventHandler<ServerStateChangedEventArgs> StateChanged;
    }

    public class ServerStateChangedEventArgs : EventArgs
    {
        public ServerState PreviousState { get; }
        public ServerState CurrentState { get; }
        public DateTime ChangedAtUtc { get; }
        public string? TriggeredBy { get; }

        public ServerStateChangedEventArgs(
            ServerState previousState,
            ServerState currentState,
            DateTime changedAtUtc,
            string? triggeredBy = null)
        {
            PreviousState = previousState;
            CurrentState = currentState;
            ChangedAtUtc = changedAtUtc;
            TriggeredBy = triggeredBy;
        }
    }
}
