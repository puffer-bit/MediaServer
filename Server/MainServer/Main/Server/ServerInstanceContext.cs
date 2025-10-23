using Server.Domain.Enums;

namespace Server.MainServer.Main.Server
{
    public class ServerInstanceContext : IServerInstanceContext
    {
        // Main properties
        public required string Id { get; init; }
        public required string Version { get; set; }
        public string? Key { get; set; }
        public string? Ip { get; set; }
        public string? DnsName { get; set; }
        public int Port { get; set; }
        public int MaxCoordinators { get; set; }
        public int MaxOnlineUsers { get; set; }
        public int MaxUsers { get; set; }

        // Time properties
        public required string TimeZoneId { get; set; }
        public DateTime FirstLaunchTime { get; set; }
        public DateTime CurrentLaunchTime { get; set; }
        public TimeSpan UpTime => DateTime.UtcNow - CurrentLaunchTime;

        // Hardware and Software parametrs
        public required string OperatingSystem { get; set; }
        public required string CPUName { get; set; }
        public required string DBProvider { get; set; }
        public string? GPUName { get; set; }
        public bool IsHardwareAccelerated { get; set; }
        public int RAMSizeMB { get; set; }
        public int MaxDBSizeMB { get; set; }

        // Region parametrs
        public required string RegionName { get; set; }
        public required string RegionCode { get; set; }
        public string? CityName { get; set; }
        public string? Locale { get; set; }


        // Statictics parametrs
        public int ActiveSessionsCount { get; set; }
        public int ConnectedUsersCount { get; set; }
        public int RegisteredUsersCount { get; set; }
        public double CpuLoadPercent { get; set; }
        public double MemoryConsumption { get; set; }

        // Security parametrs
        public ServerState State { get; set; }
    }
}
