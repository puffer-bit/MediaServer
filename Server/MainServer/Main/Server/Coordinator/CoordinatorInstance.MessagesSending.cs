using Shared.Models;

namespace Server.MainServer.Main.Server.Coordinator;

public partial class CoordinatorInstance
{
    public void SendMessageToUser(string userId, BaseMessage message)
    {
        _messageSender.SendMessageToUser(userId, message);
    }

    public void BroadcastMessage(BaseMessage message)
    {
        _messageSender.BroadcastMessage(message);
    }
}