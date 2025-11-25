using System.Collections.Concurrent;
using Server.MainServer.Main.Server.Coordinator.Sessions.Chat;
using Server.MainServer.Main.Server.Coordinator.Sessions.Video;
using Server.MainServer.Main.Server.Coordinator.Sessions.Voice;

namespace Server.MainServer.Main.Server.Coordinator.Sessions.Manager;

public interface ISessionManagerContext
{
    ConcurrentDictionary<string, IVideoSession> VideoSessions { get; set; }
    ConcurrentDictionary<string, ChatSession> ChatSessions { get; set; }
    ConcurrentDictionary<string, VoiceSession> VoiceSessions { get; set; }
}