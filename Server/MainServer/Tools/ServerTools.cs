using Newtonsoft.Json;
using Shared.Models;

namespace Server.MainServer.Tools
{
    public class ServerTools(ILogger<ServerTools> logger)
    {
        public string SerializeMessage(BaseMessage message)
        {
            logger.LogDebug("Serializing message of type: {Type}", message.Type);
            return JsonConvert.SerializeObject(message);
        }

        public BaseMessage DeSerializeMessage(string jsonMessage)
        {
            var message = JsonConvert.DeserializeObject<BaseMessage>(jsonMessage);
            logger.LogDebug("Deserialized message: {Type}", message?.Type);
            return message;
        }
    }

}
