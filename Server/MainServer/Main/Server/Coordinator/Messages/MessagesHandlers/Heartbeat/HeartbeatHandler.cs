using Server.MainServer.Main.Server.Coordinator.Connection.HeartbeatManager;
using Shared.Enums;
using Shared.Models;
using Shared.Models.Requests.Heartbeat;

namespace Server.MainServer.Main.Server.Coordinator.Messages.MessagesHandlers.Heartbeat;

public class HeartbeatHandler : IMessageHandler
{
    private readonly CoordinatorInstance _coordinator;
    private readonly ILogger<HeartbeatManager> _logger;

    public MessageType Type => MessageType.Heartbeat;
    
    public HeartbeatHandler(
        ILogger<HeartbeatManager> logger,
        CoordinatorInstance coordinator)
    {
        _logger = logger;
        _coordinator = coordinator;
    }
    
    public Task<HandleMessageResult> HandleMessage(BaseMessage message)
    {
        var request = (HeartbeatModel)message.Data;
        if (request == null)
            return Task.FromResult(HandleMessageResult.InternalError);
        
        try
        {
            return request.Type switch
            {
                HeartbeatType.Pong => Task.FromResult(HandlePong(message)),
                HeartbeatType.Ping => Task.FromResult(HandlePing()),
                HeartbeatType.Disconnected => Task.FromResult(HandleDissconected()),
                _ => Task.FromResult(HandleMessageResult.InternalError)
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Error in Heartbeat handler. Exception: {exception}", ex.Message);

            _coordinator.DetachUser(message.UserId!, "Heartbeat error", new BaseMessage(message)
            {
                Data = new HeartbeatModel(HeartbeatType.DisconnectedDueError)
            });
            return Task.FromResult(HandleMessageResult.InternalError);
        }
    }

    private HandleMessageResult HandlePong(BaseMessage message)
    {
        _coordinator.RegisterPong(message.UserId);
        return HandleMessageResult.NoError;
    }
    
    public HandleMessageResult HandlePing()
    {
        throw new NotImplementedException();
    }
    
    public HandleMessageResult HandleDissconected()
    {
        throw new NotImplementedException();

    }
}