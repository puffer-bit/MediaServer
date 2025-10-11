using Shared.Models;
using Newtonsoft.Json;

namespace Client.Services.Server.Coordinator.Messaging.MessageSender;

public class MessageSender : IMessageSender
{
    private readonly CoordinatorSession _coordinatorSession;

    public MessageSender(CoordinatorSession coordinatorSession)
    {
        _coordinatorSession = coordinatorSession;
    }

    public void SendMessage(BaseMessage message)
    {
        var webSocket = _coordinatorSession.GetWebSocket();
        if (webSocket != null)
        {
            webSocket.Send(JsonConvert.SerializeObject(message, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            }));
        }
    }
}