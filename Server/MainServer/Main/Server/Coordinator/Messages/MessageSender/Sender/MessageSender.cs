using Newtonsoft.Json;
using Shared.Models;

namespace Server.MainServer.Main.Server.Coordinator.Messages.MessageSender.Sender;

public class MessageSender : IMessageSender
{
    private readonly CoordinatorInstance _coordinator;
    
    public MessageSender(CoordinatorInstance coordinator)
    {
        _coordinator = coordinator;
    }

    public void BroadcastMessage(BaseMessage message)
    {
        foreach (var connection in _coordinator.GetAllConnections())
        {
            if (connection.Value.WebSocket.IsAvailable)
            {
                connection.Value.WebSocket.Send(SerializeMessage(message));
            }
        }
    }
    
    public void SendMessageToUser(string userId, BaseMessage message)
    {
        if (_coordinator.GetClientConnection(userId, out var connection) && connection!.WebSocket.IsAvailable)
        {
            var serilazedMessage = SerializeMessage(message);
            connection.WebSocket.Send(serilazedMessage);
        }
    }
    
    public string SerializeMessage(BaseMessage message)
    {
        return JsonConvert.SerializeObject(message, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        });
    }
}
