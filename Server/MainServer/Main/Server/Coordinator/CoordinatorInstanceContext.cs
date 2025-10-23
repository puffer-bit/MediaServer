using System.Collections.Concurrent;

namespace Server.MainServer.Main.Server.Coordinator
{
    public class CoordinatorInstanceContext : ICoordinatorInstanceContext
    {
        public required string Id { get; init; }
        public string? Ip { get; set; }
        public int? Port { get; set; }
        public DateTime FirstLaunchTime { get; set; }
        public DateTime CurrentLaunchTime { get; set; }
        public TimeSpan UpTime => DateTime.UtcNow - CurrentLaunchTime;
        public int MaxOnlineUsers { get; set; }
        public int MaxUsers { get; set; }

        // Security parametrs
        public bool IsRemoteConnectionsAvailable { get; set; }
        public bool IsRemoteConnectionsRestricted { get; set; }
        public ConcurrentBag<string> AllowedIPs { get; init; }
        public ConcurrentBag<string> BannedIPs { get; init; }

        public Task CommitContextToDBAsync()
        {
            throw new NotImplementedException();
        }

        public Task UpdateContextFromDBAsync()
        {
            throw new NotImplementedException();
        }
    }
}
