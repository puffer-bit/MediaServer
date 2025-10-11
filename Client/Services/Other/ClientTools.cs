using Shared.Enums;
using Shared.Models;
using Newtonsoft.Json;

namespace Client.Services.Other;

public class ClientTools()
{
    public string SerializeMessage(BaseMessage message)
    {
        return JsonConvert.SerializeObject(message);
    }

    public BaseMessage DeSerializeMessage(string jsonMessage)
    {
        var message = JsonConvert.DeserializeObject<BaseMessage>(jsonMessage);
        return message;
    }
}