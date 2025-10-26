using Server.Domain.Enums;

namespace Server.MainServer.Main.Server.Orchestrator
{
    public interface IServerInstance
    {
        bool IsReady { get; set; }
        IServerInstanceContext Context { get; init; }

        // Lifecycle actions
        Task StartAsync(CancellationToken ct = default);
        Task StopAsync(CancellationToken ct = default);
        Task RestartAsync(CancellationToken ct = default);
        Task EnterMaintenanceModeAsync();

        // DB Maintaince
        Task CheckDatabaseStateAsync();
        Task MigrateToNewDatabaseAsync(string targetConnectionString);
        Task BackupDatabaseAsync(string backupPath);
        Task RestoreDatabaseBackupAsync(string backupFilePath);

        // Media Maintaince
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
        Task CreateAndAttachCoordinatorInstanceAsync();
        Task DisableCoordinatorInstanceAsync(string coordinatorInstanceId);
        Task CommitCoordinatorDataAsync();
        Task RemoveCoordinatorInstanceAsync(string coordinatorInstanceId);
        Task RestoreCoordinatorInstanceAsync(string coordinatorInstanceId);
        Task PurgeCoordinatorInstanceAsync(string coordinatorInstanceId);

        // Connection actions
        void EnableRemoteConnections();
        void ResrictRemoteConnections(bool applyToActiveConnections);
        void DisableRemoteConnections();
        Task AddAllowedIPAsync(string ip);

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
