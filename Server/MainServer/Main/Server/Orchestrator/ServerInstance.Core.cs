namespace Server.MainServer.Main.Server.Orchestrator
{
    public partial class ServerInstance : IServerInstance
    {
        public bool IsReady { get; set; }
        public IServerInstanceContext Context { get; init; }
        public Task StartAsync(CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task StopAsync(CancellationToken ct = default)
        {
            throw new NotImplementedException();
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

        public Task CreateAndAttachCoordinatorInstanceAsync()
        {
            throw new NotImplementedException();
        }

        public Task DisableCoordinatorInstanceAsync(string coordinatorInstanceId)
        {
            throw new NotImplementedException();
        }

        public Task CommitCoordinatorDataAsync()
        {
            throw new NotImplementedException();
        }

        public Task RemoveCoordinatorInstanceAsync(string coordinatorInstanceId)
        {
            throw new NotImplementedException();
        }

        public Task RestoreCoordinatorInstanceAsync(string coordinatorInstanceId)
        {
            throw new NotImplementedException();
        }

        public Task PurgeCoordinatorInstanceAsync(string coordinatorInstanceId)
        {
            throw new NotImplementedException();
        }

        public void EnableRemoteConnections()
        {
            throw new NotImplementedException();
        }

        public void ResrictRemoteConnections(bool applyToActiveConnections)
        {
            throw new NotImplementedException();
        }

        public void DisableRemoteConnections()
        {
            throw new NotImplementedException();
        }

        public Task AddAllowedIPAsync(string ip)
        {
            throw new NotImplementedException();
        }

        public Task ProcessExternalAction()
        {
            throw new NotImplementedException();
        }

        public event EventHandler<ServerStateChangedEventArgs>? StateChanged;
    }
}
