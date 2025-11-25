using Shared.Enums;
using Shared.Models;
using Shared.Models.Requests.Heartbeat;

namespace Server.MainServer.Main.Server.Coordinator.Connection.HeartbeatManager;

public class HeartbeatManager : IHeartbeatManager
{
    private readonly CoordinatorInstance _coordinator;
    private readonly ILogger _logger;
    
    public HeartbeatManager(CoordinatorInstance coordinator, ILoggerFactory loggerFactory)
    {
        _coordinator = coordinator;
        _logger = loggerFactory.CreateLogger("ServerMessaging");
    }

    public async Task StartAsync(string userId, CancellationToken token)
    {
        await Task.Delay(10000);
        while (!token.IsCancellationRequested)
        {
            Ping(userId);
            
            await Task.Delay(TimeSpan.FromSeconds(5));
            if (_coordinator.GetClientConnection(userId, out var connection))
            {
                if (DateTime.UtcNow - connection!.LastPong > TimeSpan.FromSeconds(25)) // TODO: In future takes from settings
                {
                    _coordinator.DetachUser(userId, "Timed out", new BaseMessage(MessageType.Heartbeat, new HeartbeatModel(HeartbeatType.DisconnectedDueTimeOut)));
                }
            }
        }
    }

    public void RegisterPong(string userId)
    {
        if (_coordinator.GetClientConnection(userId, out var connection))
        {
            connection!.LastPong = DateTime.UtcNow;
        }
    }

    public void Ping(string userId)
    {
        _coordinator.SendMessageToUser(userId, new BaseMessage(MessageType.Heartbeat, new HeartbeatModel(HeartbeatType.Ping)));
    }
}