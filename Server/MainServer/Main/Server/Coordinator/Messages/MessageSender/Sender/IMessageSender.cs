using Shared.Models;

namespace Server.MainServer.Main.Server.Coordinator.Messages.MessageSender.Sender;

public interface IMessageSender
{
    void SendMessageToUser(string userId, BaseMessage message);
    void BroadcastMessage(BaseMessage message);
}