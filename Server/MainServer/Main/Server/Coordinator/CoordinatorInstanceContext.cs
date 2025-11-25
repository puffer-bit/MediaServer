using System.Collections.Concurrent;
using Server.Domain.Entities;

namespace Server.MainServer.Main.Server.Coordinator
{
    public class CoordinatorInstanceContext : ICoordinatorInstanceContext
    {
        public required string Id { get; init; }
        public required string Name { get; set; }
        public string? Ip { get; set; }
        public int? Port { get; set; }
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
        public ConcurrentBag<string> AllowedIPs { get; init; }
        public ConcurrentBag<string> BannedIPs { get; init; }

        public CoordinatorInstanceContext(string coordinatorInstanceId)
        {
            Id = coordinatorInstanceId;
        }
        
        public void LoadContext(CoordinatorInstanceEntity coordinatorInstanceEntity)
        {
            CurrentLaunchTime = DateTime.UtcNow;
            Ip = coordinatorInstanceEntity.Ip;
            Port = coordinatorInstanceEntity.Port;
            Name = coordinatorInstanceEntity.Name;
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
