using System.Net.Sockets;
using Server.Domain.Entities;
using Server.Domain.Enums;
using Server.MainServer.Main.Server.Coordinator.Sessions.Video;
using Shared.Enums;
using Shared.Models;
using Shared.Models.DTO;

namespace Server.MainServer.Main.Server.Coordinator;

public partial class CoordinatorInstance
{
    public async Task Configure(ILoggerFactory loggerFactory, CoordinatorInstanceEntity coordinatorInstanceEntity)
    {
        if (IsReady)
            return;
            
        _logger.LogInformation("Coordinator {Name} (ID: {Id}) starting...", Context.Name, Context.Id);

        try
        {
            CreateWebSocketServer(loggerFactory);
            await CheckTurnAndStunServer();
            _userManager.AddUser(new UserDTO()
            {
                Id = "system",
                Username = "System",
                UserIdentity = "system",
                Ip = "127.0.0.1",
                CoordinatorInstanceId = Context.Id,
                State = UserState.Normal
            }, out _);
                
            IsReady = true;
            IsStarted = true;
            State = CoordinatorState.Online;
            
            _logger.LogInformation("Coordinator {Name} (ID: {Id}) successful started.", Context.Name, Context.Id);
        }
        catch (Exception ex)
        {
            State = CoordinatorState.Failed;
            _logger.LogError("Coordinator {Name} (ID: {Id}) failed. Exception: {ExceptionMessage}", Context.Name, Context.Id, ex.Message);
            CloseCoordinator();
        }
    }
    
    public async Task LoadSessions(List<VideoSessionEntity> videoSessionsEntities)
    {
        _sessionManager.FetchVideoSessionsFromList(videoSessionsEntities);
    }
}