using Shared.Models;

namespace Client.Services.Server.Coordinator.Messaging.MessageSender;

public interface IMessageSender
{
    void SendMessage(BaseMessage message);
}