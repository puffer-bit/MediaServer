using System.Collections.Concurrent;
using System.Net;
using Server.Domain.Entities;

namespace Server.MainServer.Main.Server.Coordinator
{
    public class CoordinatorInstanceContext : ICoordinatorInstanceContext
    {
        public required string Id { get; init; }
        public required string Name { get; set; }
        public string Ip { get; set; }
        public int Port { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime FirstLaunchTime { get; set; }
        public DateTime CurrentLaunchTime { get; set; }
        public TimeSpan UpTime => DateTime.UtcNow - CurrentLaunchTime;
        public int MaxOnlineUsers { get; set; }
        public int MaxUsers { get; set; }
        
        // Security parameters
        public bool IsRemoteConnectionsAvailable { get; set; }
        public bool IsRemoteConnectionsRestricted { get; set; }
        public bool IsMOTDEnabled { get; set; }
        public bool IsFirstLaunch { get; set; }
        public ConcurrentBag<string> AllowedIPs { get; init; } = new();
        public ConcurrentBag<string> BannedIPs { get; init; } = new();
        
        // STUN and TURN
        public bool IsTurnEnabled { get; set; }
        public string? TurnAddress { get; set; }
        public ushort? TurnPort { get; set; }
        public string? TurnUsername { get; set; }
        public string? TurnPassword { get; set; }
        
        public bool IsStunEnabled { get; set; }
        public string? StunAddress { get; set; }
        public ushort? StunPort { get; set; }
        
        public string ServerVersion { get; init; }

        public CoordinatorInstanceContext(string coordinatorInstanceId, string serverVersion)
        {
            Id = coordinatorInstanceId;
            ServerVersion = serverVersion;
        }
        
        public void LoadContext(CoordinatorInstanceEntity coordinatorInstanceEntity)
        {
            CurrentLaunchTime = DateTime.UtcNow;
            
            Ip = coordinatorInstanceEntity.Ip;
            Port = coordinatorInstanceEntity.Port;
            Name = coordinatorInstanceEntity.Name;
            
            IsStunEnabled = coordinatorInstanceEntity.IsStunEnabled;
            if (coordinatorInstanceEntity.StunAddress == null
                || coordinatorInstanceEntity.StunPort == 0
                || !IPAddress.TryParse(coordinatorInstanceEntity.StunAddress, out _))
            {
                IsStunEnabled = false;
            }
            else
            {
                StunAddress = coordinatorInstanceEntity.StunAddress;
                StunPort = coordinatorInstanceEntity.StunPort;
            }
            
            IsTurnEnabled = coordinatorInstanceEntity.IsTurnEnabled;
            if (coordinatorInstanceEntity.TurnAddress == null
                || coordinatorInstanceEntity.TurnPort == 0
                || !IPAddress.TryParse(coordinatorInstanceEntity.TurnAddress, out _))
            {
                IsTurnEnabled = false;
            }
            else
            {
                TurnAddress = coordinatorInstanceEntity.TurnAddress;
                TurnPort = coordinatorInstanceEntity.TurnPort;
                TurnUsername = coordinatorInstanceEntity.TurnUsername;
                TurnPassword = coordinatorInstanceEntity.TurnPassword;
            }
        }

        public void CommitContext()
        {
            throw new NotImplementedException();
        }

        public CoordinatorInstanceEntity GetAsEntity()
        {
            throw new NotImplementedException();
        }
    }
}
