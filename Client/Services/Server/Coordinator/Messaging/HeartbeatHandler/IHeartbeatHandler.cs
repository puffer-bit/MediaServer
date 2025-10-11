namespace Client.Services.Server.Coordinator.Messaging.HeartbeatHandler;

public interface IHeartbeatHandler
{
    void ReactOnPing();
    bool IsServerAlive();
}