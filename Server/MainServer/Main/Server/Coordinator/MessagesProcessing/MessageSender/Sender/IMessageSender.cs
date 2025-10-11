using Shared.Models;

namespace Server.MainServer.Main.Server.Coordinator.MessagesProcessing.MessageSender.Sender;

public interface IMessageSender
{
    void SendMessageToUser(string userId, BaseMessage message);
    void BroadcastMessage(BaseMessage message);
}