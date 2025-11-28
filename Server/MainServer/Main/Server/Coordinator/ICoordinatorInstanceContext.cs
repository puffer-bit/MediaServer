using System.Collections.Concurrent;
using Server.Domain.Entities;

namespace Server.MainServer.Main.Server.Coordinator;

public interface ICoordinatorInstanceContext
{
    // Main parameters
    string Id { get; init; }
    string Name { get; set; }
    string Ip { get; set; }
    int Port { get; set; }
    DateTime CreateTime { get; set; }
    DateTime FirstLaunchTime { get; set; }
    DateTime CurrentLaunchTime { get; set; }
    int MaxOnlineUsers { get; set; }
    int MaxUsers { get; set; }

    // Security parameters
    bool IsMOTDEnabled { get; set; }
    ConcurrentBag<string> AllowedIPs { get; init; }
    ConcurrentBag<string> BannedIPs { get; init; }
    
    // STUN and TURN
    bool IsTurnEnabled { get; set; }
    string? TurnAddress { get; set; }
    ushort? TurnPort { get; set; }
    string? TurnUsername { get; set; }
    string? TurnPassword { get; set; }
        
    bool IsStunEnabled { get; set; }
    string? StunAddress { get; set; }
    ushort? StunPort { get; set; }
    
    string ServerVersion { get; init; }

    void LoadContext(CoordinatorInstanceEntity coordinatorInstanceEntity);
    void CommitContext();
    CoordinatorInstanceEntity GetAsEntity();
}