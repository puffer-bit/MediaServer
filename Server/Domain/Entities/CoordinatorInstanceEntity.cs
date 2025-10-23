using System.Collections.Concurrent;

namespace Server.Domain.Entities
{
    public class CoordinatorInstanceEntity
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
        bool IsRemoteConnectionsAvailable { get; set; }
        bool IsRemoteConnectionsRestricted { get; set; }
        ConcurrentBag<string> AllowedIPs { get; init; }
        ConcurrentBag<string> BannedIPs { get; init; }

        public CoordinatorInstanceEntity(string id)
        {
            Id = id;
        }
    }
}
