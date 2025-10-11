using System.Collections.Concurrent;
using Server.MainServer.Main.Server.Chat;
using Server.MainServer.Main.Server.Video;
using Server.MainServer.Main.Server.Voice;

namespace Server.MainServer.Main.Server.Coordinator.Manager;

public class SessionManagerContext : ISessionManagerContext
{
    public ConcurrentDictionary<string, IVideoSession> VideoSessions { get; set; } = new();
    public ConcurrentDictionary<string, ChatSessionContext> ChatSessions { get; set; } = new();
    public ConcurrentDictionary<string, VoiceSession> VoiceSessions { get; set; } = new();
}