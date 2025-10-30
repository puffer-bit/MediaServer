using Newtonsoft.Json.Linq;
using Server.MainServer.Main.Server.Coordinator;
using Server.MainServer.Main.Server.Coordinator.MessagesProcessing.MessagesHandlers;
using Shared.Enums;
using Shared.Models;
using Shared.Models.Requests.SessionActionsRequests;

namespace Server.SFU.Main.Server.Coordinator.MessagesProcessing.MessagesHandlers.SessionActions;

public class SessionActionMessageHandler : IMessageHandler
{
    private readonly CoordinatorInstance _coordinator;
    private readonly ILogger<SessionActionMessageHandler> _logger;

    public MessageType Type => MessageType.RoomRequest;
    
    public SessionActionMessageHandler(
        ILogger<SessionActionMessageHandler> logger,
        CoordinatorInstance coordinator)
    {
        _logger = logger;
        _coordinator = coordinator;
    }

    public Task<HandleMessageResult> HandleMessage(BaseMessage message)
    {
        try
        {
            return message.Data switch
            {
                CreateSessionRequestModel request => Task.FromResult(HandleCreate(request, message)),
                JoinSessionRequestModel request => Task.FromResult(HandleJoin(request, message)),
                LeaveSessionRequestModel request => Task.FromResult(HandleLeave(request, message)),
                DeleteSessionRequestModel request => Task.FromResult(HandleDelete(request, message)),
                KickFromSessionRequestModel request => Task.FromResult(HandleKick(request, message)),
                BanFromSessionRequestModel request => Task.FromResult(HandleBan(request, message)),
                ApproveUserRequestModel request => Task.FromResult(HandleApprove(request, message)),
                RejectUserRequestModel request => Task.FromResult(HandleReject(request, message)),
                SessionRequestType.Reconfigure => throw new NotImplementedException(),
                _ => Task.FromResult(HandleMessageResult.InternalError)
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Error in RoomAction handler. Exception: {exception}", ex.Message);
            var response = new UndefinedSessionRequestModel()
            {
                Session = new SessionDTO()
                {
                    Id = "-",
                    Name = "-",
                    HostId = "-",
                    Capacity = 0,
                    SessionType = SessionType.Undefiend
                }
            };

            _coordinator.SendMessageToUser(message.UserId!, new BaseMessage(message)
            {
                Data = response
            });
            return Task.FromResult(HandleMessageResult.InternalError);
        }
    }

    private HandleMessageResult HandleCreate(IUserSessionRequestModel request, BaseMessage message)
    {
        var response = (CreateSessionRequestModel)request;
        response.Result = _coordinator.CreateSession(response.Session, message.UserId!);

        _coordinator.SendMessageToUser(message.UserId!, new BaseMessage(message)
        {
            Data = response
        });

        return response.Result == CreateSessionResult.NoError
            ? HandleMessageResult.NoError
            : HandleMessageResult.InternalError;
    }

    private HandleMessageResult HandleJoin(IUserSessionRequestModel request, BaseMessage message)
    {
        var response = (JoinSessionRequestModel)request;
        response.Result = _coordinator.EnterSession(request.Session, message.UserId!);
        if (_coordinator.GetVideoSession(request.Session.Id, out var foundedSession) == SessionRequestResult.NoError)
        {
            if (foundedSession!.GetPeerByUserId(message.UserId!, out var foundedPeer))
            {
                response.PeerId = foundedPeer!.GetId();
            }
        }

        _coordinator.SendMessageToUser(message.UserId!, new BaseMessage(message)
        {
            Data = response
        });

        return response.Result == JoinSessionResult.NoError
            ? HandleMessageResult.NoError
            : HandleMessageResult.InternalError;
    }

    private HandleMessageResult HandleLeave(IUserSessionRequestModel request, BaseMessage message)
    {
        var response = (LeaveSessionRequestModel)request;
        response.Result = _coordinator.KickFromSession(request.Session, message.UserId!, false);

        // _coordinator.SendMessageToUser(message.UserId!, new BaseMessage(message)
        // {
        //     Data = response
        // });

        return response.Result == LeaveSessionResult.NoError
            ? HandleMessageResult.NoError
            : HandleMessageResult.InternalError;
    }

    private HandleMessageResult HandleDelete(IUserSessionRequestModel request, BaseMessage message)
    {
        var response = (DeleteSessionRequestModel)request;
        response.Result = _coordinator.DeleteSession(response.Session, message.UserId!);

        _coordinator.SendMessageToUser(message.UserId!, new BaseMessage(message)
        {
            Data = response
        });

        return response.Result == DeleteSessionResult.NoError
            ? HandleMessageResult.NoError
            : HandleMessageResult.InternalError;
    }

    private HandleMessageResult HandleKick(IUserSessionRequestModel request, BaseMessage message)
    {
        var response = (KickFromSessionRequestModel)request;
        response.Result = _coordinator.KickFromSession(response.Session, message.UserId!, true);

        _coordinator.SendMessageToUser(message.UserId!, new BaseMessage(message)
        {
            Data = response
        });

        return response.Result == LeaveSessionResult.NoError
            ? HandleMessageResult.NoError
            : HandleMessageResult.InternalError;
    }
    
    private HandleMessageResult HandleBan(IUserSessionRequestModel request, BaseMessage message)
    {
        var response = (BanFromSessionRequestModel)request;
        //response.Result = _coordinator.BanFromSession(response.Session, message.UserId!);
        throw new NotImplementedException();

        _coordinator.SendMessageToUser(message.UserId!, new BaseMessage(message)
        {
            Data = response
        });

        return response.Result == BanFromSessionResult.NoError
            ? HandleMessageResult.NoError
            : HandleMessageResult.InternalError;
    }
    
    private HandleMessageResult HandleApprove(IUserSessionRequestModel request, BaseMessage message)
    {
        var response = (ApproveUserRequestModel)request;
        //response.Result = _coordinator.ApproveUserInSession(response.Session, message.UserId!);
        throw new NotImplementedException();

        _coordinator.SendMessageToUser(message.UserId!, new BaseMessage(message)
        {
            Data = response
        });

        return response.Result == ApproveUserSessionResult.NoError
            ? HandleMessageResult.NoError
            : HandleMessageResult.InternalError;
    }
    
    private HandleMessageResult HandleReject(IUserSessionRequestModel request, BaseMessage message)
    {
        var response = (RejectUserRequestModel)request;
        //response.Result = _coordinator.RejectUserInSession(response.Session, message.UserId!);
        throw new NotImplementedException();

        _coordinator.SendMessageToUser(message.UserId!, new BaseMessage(message)
        {
            Data = response
        });

        return response.Result == RejectUserSessionResult.NoError
            ? HandleMessageResult.NoError
            : HandleMessageResult.InternalError;
    }
}