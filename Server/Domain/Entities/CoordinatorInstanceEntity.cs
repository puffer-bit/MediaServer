using System.Collections.Concurrent;
using Server.Domain.Enums;

namespace Server.Domain.Entities
{
    public class CoordinatorInstanceEntity
    {
        public required string Id { get; init; }
        public required string Name { get; set; }
        public string Ip { get; set; }
        public int Port { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime FirstLaunchTime { get; set; }
        public DateTime CurrentLaunchTime { get; set; }
        public int MaxOnlineUsers { get; set; }
        public int MaxUsers { get; set; }

        // Security parametrs
        public bool IsMOTDEnabled { get; set; }
        List<string> AllowedIPs { get; init; }
        List<string> BannedIPs { get; init; }
        
        // STUN and TURN
        public bool IsTurnEnabled { get; set; }
        public string? TurnAddress { get; set; }
        public ushort? TurnPort { get; set; }
        public string? TurnUsername { get; set; }
        public string? TurnPassword { get; set; }
        
        public bool IsStunEnabled { get; set; }
        public string? StunAddress { get; set; }
        public ushort? StunPort { get; set; }
    }
}
