using System.Collections.Concurrent;
using System.Reflection;
using Server.Domain.Entities;
using Server.Domain.Enums;

namespace Server.MainServer.Main.Server.Orchestrator
{
    public class OrchestratorInstanceContext : IOrchestratorInstanceContext
    {
        // Main properties
        public required int Id { get; init; } = 1;
        public required string ServerVersion { get; set; }
        public string? Key { get; set; }
        public string Ip { get; set; }
        public string? DnsName { get; set; }
        public int Port { get; set; }
        public int MaxCoordinators { get; set; }
        public int MaxOnlineUsers { get; set; }
        public int MaxUsers { get; set; }

        // Time properties
        public required string TimeZoneId { get; set; }
        public DateTime FirstLaunchTime { get; set; }
        public DateTime CurrentLaunchTime { get; set; }

        // Hardware and Software parameters
        public required string OperatingSystem { get; set; }
        public required string CpuName { get; set; }
        public required string DbProvider { get; set; }
        public string? GpuName { get; set; }
        public int RamSizeMb { get; set; }
        public int MaxDbSizeMb { get; set; }

        // Region parameters
        public required string RegionName { get; set; }
        public required string RegionCode { get; set; }
        public string? CityName { get; set; }
        public string? Locale { get; set; }


        // Statistics parameters
        public int ActiveSessionsCount { get; set; }
        public int ConnectedUsersCount { get; set; }
        public int RegisteredUsersCount { get; set; }
        public double CurrentCpuLoadPercent { get; set; }
        public double CurrentMemoryConsumption { get; set; }

        // Security parameters
        public ConcurrentBag<string> AllowedIPs { get; init; } = new();
        public ConcurrentBag<string> BannedIPs { get; init; } = new();

        public void CreateContext()
        {
            if (Assembly.GetExecutingAssembly().GetName().Version == null)
                ServerVersion = "Unknown";
            else
                ServerVersion = Assembly.GetExecutingAssembly().GetName().Version!.ToString();
            
            FirstLaunchTime = DateTime.UtcNow;
            Ip = "127.0.0.1";
            Port = 2222;
        }
        
        public void RecreateContext()
        {
            if (Assembly.GetExecutingAssembly().GetName().Version == null)
                ServerVersion = "Unknown";
            else
                ServerVersion = Assembly.GetExecutingAssembly().GetName().Version!.ToString();
            
            CurrentLaunchTime = DateTime.UtcNow;
            Ip = "0.0.0.0";
            Port = 2222;
        }

        public void LoadContext(OrchestratorInstanceEntity orchestratorInstanceEntity)
        {
            throw new NotImplementedException();
        }

        public void UpdateContext(OrchestratorInstanceEntity orchestratorInstanceEntity)
        {
            ServerVersion = orchestratorInstanceEntity.ServerVersion;
            Ip = orchestratorInstanceEntity.Ip;
            Port = orchestratorInstanceEntity.Port;
        }

        public OrchestratorInstanceEntity GetAsEntity()
        {
            return new OrchestratorInstanceEntity()
            {
                ServerVersion = this.ServerVersion,
                Ip = this.Ip,
                Port = this.Port,
            };
        }
    }
}
