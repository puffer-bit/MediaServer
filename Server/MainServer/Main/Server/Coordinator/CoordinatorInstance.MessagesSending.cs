using Shared.Models;

namespace Server.MainServer.Main.Server.Coordinator;

public partial class CoordinatorInstance
{
    public void SendMessageToUser(string userId, BaseMessage message)
    {
        _messageSender.SendMessageToUser(userId, message);
    }
    
    /// <summary>
    /// Sends message to ALL users in coordinator instance
    /// </summary>
    /// <param name="message"></param>
    public void BroadcastMessage(BaseMessage message)
    {
        _messageSender.BroadcastMessage(message);
    }

    /// <summary>
    /// Sends message to users in selected session
    /// </summary>
    /// <param name="message"></param>
    /// <param name="sessionId"></param>
    public void BroadcastMessage(BaseMessage message, string sessionId)
    {
        _messageSender.BroadcastMessage(message);
    }
}