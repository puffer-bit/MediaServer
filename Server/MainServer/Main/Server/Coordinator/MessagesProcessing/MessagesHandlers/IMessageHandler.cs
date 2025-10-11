using Shared.Enums;
using Shared.Models;

namespace Server.MainServer.Main.Server.Coordinator.MessagesProcessing.MessagesHandlers;

public interface IMessageHandler
{
    MessageType Type { get; }
    Task<HandleMessageResult> HandleMessage(BaseMessage message);
}