using System;
using Shared.Models;
using Newtonsoft.Json;
using Websocket.Client;

namespace Client.Services.Server.Coordinator.Messaging.MessageRouter;

public class MessageHandler : IMessageHandler
{
    private readonly CoordinatorSession _coordinatorSession;
    private IDisposable? _messageSubscription;
    
    public MessageHandler(CoordinatorSession coordinatorSession)
    {
        _coordinatorSession = coordinatorSession;
    }

    public void StartRouting(WebsocketClient? client)
    {
        _messageSubscription?.Dispose();
        _messageSubscription = null;
        _messageSubscription = client.MessageReceived.Subscribe(async msg =>
        {
            var message = JsonConvert.DeserializeObject<BaseMessage>(msg.Text, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });
            await _coordinatorSession.ProcessEvent(message);
        });
    }
}
