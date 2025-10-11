using System.Collections.Concurrent;
using Server.MainServer.Main.Server.Chat;
using Server.MainServer.Main.Server.Video;
using Server.MainServer.Main.Server.Voice;

namespace Server.MainServer.Main.Server.Coordinator.Manager;

public interface ISessionManagerContext
{
    ConcurrentDictionary<string, IVideoSession> VideoSessions { get; set; }
    ConcurrentDictionary<string, ChatSessionContext> ChatSessions { get; set; }
    ConcurrentDictionary<string, VoiceSession> VoiceSessions { get; set; }
}