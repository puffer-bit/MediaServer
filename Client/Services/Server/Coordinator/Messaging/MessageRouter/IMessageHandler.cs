using Websocket.Client;

namespace Client.Services.Server.Coordinator.Messaging.MessageRouter;

public interface IMessageHandler
{
    void StartRouting(WebsocketClient? client);
}