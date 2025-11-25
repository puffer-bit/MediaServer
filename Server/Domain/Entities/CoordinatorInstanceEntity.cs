using System.Collections.Concurrent;
using Server.Domain.Enums;

namespace Server.Domain.Entities
{
    public class CoordinatorInstanceEntity
    {
        public required string Id { get; init; }
        public required string Name { get; set; }
        public string? Ip { get; set; }
        public int? Port { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime FirstLaunchTime { get; set; }
        public DateTime CurrentLaunchTime { get; set; }
        public int MaxOnlineUsers { get; set; }
        public int MaxUsers { get; set; }

        // Security parametrs
        public bool IsMOTDEnabled { get; set; }
        ConcurrentBag<string> AllowedIPs { get; init; }
        ConcurrentBag<string> BannedIPs { get; init; }
    }
}
