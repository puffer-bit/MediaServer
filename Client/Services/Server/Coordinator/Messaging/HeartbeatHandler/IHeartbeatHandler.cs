using System.Threading.Tasks;

namespace Client.Services.Server.Coordinator.Messaging.HeartbeatHandler;

public interface IHeartbeatHandler
{
    void ReactOnPing();
    Task ReactOnServerShutdown();
    bool IsServerAlive();
}