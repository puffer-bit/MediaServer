namespace Server.MainServer.Main.Server.Coordinator.Connection.HeartbeatManager;

public interface IHeartbeatManager
{
    Task StartAsync(string userId, CancellationToken token);
    void RegisterPong(string userId);
    void Ping(string userId);
}