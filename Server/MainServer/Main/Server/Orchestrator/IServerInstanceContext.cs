using Server.Domain.Enums;
using System.Collections.Concurrent;

namespace Server.MainServer.Main.Server.Orchestrator
{
    public interface IServerInstanceContext
    {
        // Main properties
        string Id { get; init; }
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
        TimeSpan UpTime => DateTime.UtcNow - CurrentLaunchTime;

        // Hardware and Software parametrs
        string OperatingSystem { get; set; }
        string CPUName { get; set; }
        string DBProvider { get; set; }
        string? GPUName { get; set; }
        bool IsHardwareAccelerated { get; set; }
        public int RAMSizeMB { get; set; }
        int MaxDBSizeMB { get; set; }

        // Region parametrs
        string RegionName { get; set; }
        string RegionCode { get; set; }
        string? CityName { get; set; }
        string? Locale { get; set; }

        // Statictics parametrs
        int ActiveSessionsCount { get; set; }
        int ConnectedUsersCount { get; set; }
        int RegisteredUsersCount { get; set; }

        double CurrentCpuLoadPercent { get; set; }
        double CurrentMemoryConsumption { get; set; }

        // Security parametrs
        ServerState State { get; set; }
        bool IsRemoteConnectionsAvailable { get; set; }
        bool IsRemoteConnectionsRestricted { get; set; }
        ConcurrentBag<string> AllowedIPs { get; init; }
        ConcurrentBag<string> BannedIPs { get; init; }

        // DB actions
        Task UpdateContextFromDBAsync();
        Task CommitContextToDBAsync();
    }
}
