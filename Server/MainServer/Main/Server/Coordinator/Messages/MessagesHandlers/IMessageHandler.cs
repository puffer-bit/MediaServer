using Shared.Enums;
using Shared.Models;

namespace Server.MainServer.Main.Server.Coordinator.Messages.MessagesHandlers;

public interface IMessageHandler
{
    MessageType Type { get; }
    Task<HandleMessageResult> HandleMessage(BaseMessage message);
}