using Server.Domain.Enums;
using System.Collections.Concurrent;
using Server.Domain.Entities;

namespace Server.MainServer.Main.Server.Orchestrator
{
    public interface IOrchestratorInstanceContext
    {
        // Main properties
        int Id { get; init; }
        string Version { get; set; }
        string? Key { get; set; }
        string? Ip { get; set; }
        string? DnsName { get; set; }
        int Port { get; set; }
        int MaxCoordinators { get; set; }
        int MaxOnlineUsers { get; set; }
        int MaxUsers { get; set; }

        // Time properties
        string TimeZoneId { get; set; }
        DateTime FirstLaunchTime { get; set; }
        DateTime CurrentLaunchTime { get; set; }

        // Hardware and Software parametrs
        string OperatingSystem { get; set; }
        string CpuName { get; set; }
        string DbProvider { get; set; }
        string? GpuName { get; set; }

        // Region parametrs
        string RegionName { get; set; }
        string RegionCode { get; set; }
        string? CityName { get; set; }
        string? Locale { get; set; }
        
        // Security parametrs
        ConcurrentBag<string> AllowedIPs { get; init; }
        ConcurrentBag<string> BannedIPs { get; init; }
        
        // DB actions
        void CreateContext();
        void RecreateContext();
        void LoadContext(OrchestratorInstanceEntity orchestratorInstanceEntity);
        void UpdateContext(OrchestratorInstanceEntity orchestratorInstanceEntity);
        OrchestratorInstanceEntity GetAsEntity();
    }
}
