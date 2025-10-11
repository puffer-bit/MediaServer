using Fleck;
using Newtonsoft.Json;
using Server.MainServer.Main.Server.Coordinator;
using Shared.Enums;
using Shared.Models;

namespace Server.MainServer.Main.WebSocket
{
    public class MainWebSocket
    {
        private readonly CoordinatorInstance _coordinator;
        private readonly IWebSocketConnection _socket;
        private readonly ILogger _logger;
        private UserDTO? User { get; set; }
        
        public MainWebSocket(CoordinatorInstance coordinatorInstance, IWebSocketConnection socket, ILoggerFactory loggerFactory)
        {
            _coordinator = coordinatorInstance;
            _socket = socket;
            _logger = loggerFactory.CreateLogger("Coordinator");
        }
        
        public Action<string> OnMessage => (e) =>
        {
            _logger.LogTrace("OnMessage: {e}", e);

            var message = DeSerializeMessage(e);
            
            if (User == null)
            {
                if (message.Type == MessageType.UserAuth)
                {
                    // Проверка пользователя

                    User = (UserDTO)message.Data;
                    _coordinator.AttachUser(message, _socket);
                    
                    _logger.LogDebug("User {Name} connected.", User.Name);
                }
                else
                {
                    _logger.LogError("Connection closed. User must be validated.");
                    _socket.Close();
                }
            }
            else
            {
                _coordinator.ProcessEvent(message);
            }
        };

        public Action OnOpen => () =>
        {
            // _logger.LogDebug(
            //     "Web socket client connection from {UserEndPoint}, waiting client auth...",
            //     _socket.ConnectionInfo.ClientIpAddress);
        };

        public Action<Exception> OnError => (e) => 
        {
            _logger.LogError("Error on ws connection. {e}",e.Message);
        };

        public Action OnClose => () =>
        {
            if (User != null)
            {
                _coordinator.DetachUser(User.Id, "WebSocket disconnection.");
            }
        };
        
        private BaseMessage? DeSerializeMessage(string jsonMessage)
        {
            return JsonConvert.DeserializeObject<BaseMessage>(jsonMessage, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });
        }
        
        private string SerializeMessage(BaseMessage message)
        {
            return JsonConvert.SerializeObject(message, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });
        }
    }
}

