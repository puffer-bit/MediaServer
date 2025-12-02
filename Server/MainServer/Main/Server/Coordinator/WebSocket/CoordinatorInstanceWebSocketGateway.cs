using Fleck;
using Newtonsoft.Json;
using Shared.Enums;
using Shared.Models;
using Shared.Models.DTO;

namespace Server.MainServer.Main.Server.Coordinator.WebSocket
{
    public class CoordinatorInstanceWebSocketGateway
    {
        private readonly CoordinatorInstance _coordinator;
        private readonly IWebSocketConnection _socket;
        private readonly ILogger _logger;
        private UserDTO? User { get; set; }
        
        public CoordinatorInstanceWebSocketGateway(CoordinatorInstance coordinatorInstance, IWebSocketConnection socket, ILoggerFactory loggerFactory)
        {
            _coordinator = coordinatorInstance;
            _socket = socket;
            _logger = loggerFactory.CreateLogger($"Coordinator {coordinatorInstance.Context.Id} WebSocket Gateway");
        }
        
        public Action<string> OnMessage => (e) =>
        {
            //_logger.LogTrace("OnMessage: {e}", e);

            var message = DeSerializeMessage(e);
            
            if (message == null)
                return;
            
            if (User == null)
            {
                if (message.Type == MessageType.UserAuth)
                {
                    // Проверка пользователя

                    User = (UserDTO)message.Data;
                    _coordinator.AttachUser(message, _socket);
                    
                    _logger.LogTrace("User {Name}(ID:{Id}) connected.", User.Username, User.Id);
                }
                else
                {
                    _logger.LogDebug("Connection closed. User cannot be identified.");
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
            _logger.LogTrace(
                "Web socket client connection from {UserEndPoint}.",
                _socket.ConnectionInfo.ClientIpAddress);
        };

        public Action<Exception> OnError => (e) => 
        {
            _logger.LogError("Error on ws connection. {e}",e.Message);
        };

        public Action OnClose => () =>
        {

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

