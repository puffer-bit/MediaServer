using System.Collections.Concurrent;
using Server.MainServer.Main.Server.Coordinator.Sessions.Chat;
using Server.MainServer.Main.Server.Coordinator.Sessions.Video;
using Server.MainServer.Main.Server.Coordinator.Sessions.Voice;

namespace Server.MainServer.Main.Server.Coordinator.Sessions.Manager;

public class SessionManagerContext : ISessionManagerContext
{
    public ConcurrentDictionary<string, IVideoSession> VideoSessions { get; set; } = new();
    public ConcurrentDictionary<string, ChatSession> ChatSessions { get; set; } = new();
    public ConcurrentDictionary<string, VoiceSession> VoiceSessions { get; set; } = new();
}