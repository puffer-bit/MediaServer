using System.Collections.Concurrent;

namespace Server.MainServer.Main.Server.Coordinator;

public interface ICoordinatorInstanceContext
{
    // Main parametrs
    string Id { get; init; }
    string? Ip { get; set; }
    int? Port { get; set; }
    DateTime FirstLaunchTime { get; set; }
    DateTime CurrentLaunchTime { get; set; }
    TimeSpan UpTime => DateTime.UtcNow - CurrentLaunchTime;
    int MaxOnlineUsers { get; set; }
    int MaxUsers { get; set; }

    // Security parametrs
    bool IsRemoteConnectionsAvailable { get; set; }
    bool IsRemoteConnectionsRestricted { get; set; }
    ConcurrentBag<string> AllowedIPs { get; init; }
    ConcurrentBag<string> BannedIPs { get; init; }

    Task UpdateContextFromDBAsync();
    Task CommitContextToDBAsync();
}