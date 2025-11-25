using Shared.Enums;
using Shared.Models;
using SIPSorcery.Net;

namespace Server.MainServer.Main.Server.Coordinator.Messages.MessagesHandlers.WebRTCNegotiation
{
    public class WebRTCNegotiationMessagesHandler : IMessageHandler
    {
        private readonly CoordinatorInstance _coordinator;
        private readonly ILogger _logger;
        
        public MessageType Type => MessageType.WebRTCInit;
        
        public WebRTCNegotiationMessagesHandler(
            ILoggerFactory loggerFactory,
            CoordinatorInstance coordinator)
        {
            _coordinator = coordinator;
            _logger = loggerFactory.CreateLogger("SessionManager");
        }
        
        public async Task<HandleMessageResult> HandleMessage(BaseMessage message)
        {
            var negotiationRequest = (Shared.Models.Requests.WebRTCNegotiation)message.Data;
            try
            {
                return negotiationRequest.Type switch
                {
                    WebRTCNegotiationType.Offer =>  HandleOffer(negotiationRequest, message),
                    WebRTCNegotiationType.OfferRequest => await HandleOfferRequest(negotiationRequest, message),
                    WebRTCNegotiationType.Answer =>  HandleAnswer(negotiationRequest, message),
                    WebRTCNegotiationType.ICE =>  HandleICE(negotiationRequest, message),
                    WebRTCNegotiationType.Renegotiation => await HandleRenegotiation(negotiationRequest, message),
                    _ => HandleMessageResult.InternalError
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in RoomAction handler. Exception: {exception}", ex.Message);
                SendResponse(negotiationRequest, WebRTCNegotiationResult.InternalError, message);
                return HandleMessageResult.InternalError;
            }
        }

        private async Task<HandleMessageResult> HandleRenegotiation(Shared.Models.Requests.WebRTCNegotiation request, BaseMessage message)
        {
            _logger.LogTrace("Renegotiation event initiated.");
            if (_coordinator.GetVideoSession(request.RoomId, out var session) == SessionRequestResult.NoError)
            {
                _coordinator.CreateNewWebRTCPeer(message.UserId, request.RoomId, request.PeerId, false, session!.IsAudioRequested);
                var offerSdp = new Shared.Models.Requests.WebRTCNegotiation(
                    WebRTCNegotiationType.Renegotiation,
                    request.PeerId, request.RoomId,
                    await _coordinator.ReactOnOfferRequest(request.PeerId));
                SendResponse(offerSdp, WebRTCNegotiationResult.NoError, message);
                return HandleMessageResult.NoError;
            }

            _logger.LogDebug("ReactOnOfferRequest event error. Requested room not exist.");
            SendResponse(request, WebRTCNegotiationResult.InternalError, message);
            return HandleMessageResult.InternalError;
        }

        /// <summary>
        ///  Handle offer
        /// </summary>
        /// <param name="request"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private HandleMessageResult HandleOffer(Shared.Models.Requests.WebRTCNegotiation request, BaseMessage message)
        {
            _logger.LogTrace("ReactOnOffer event initiated.");
            var sdpOffer = (RTCSessionDescriptionInit)request.Data!;
            if (_coordinator.GetVideoSession(request.RoomId, out var session) == SessionRequestResult.NoError)
            {
                if (session.GetPeerById(request.PeerId, out _))
                {
                    _coordinator.ReactOnOffer(request.PeerId, sdpOffer);
                    throw new NotImplementedException();
                    //var sdpAnswer = _webRTCManager.createAnswer();
                    //session.SendAnswer(sdpAnswer, request.PeerId);
                    _logger.LogInformation("ReactOnOffer event completed.");
                    return HandleMessageResult.NoError;
                }

                _logger.LogDebug("ReactOnOffer event error. Requested peer not exist.");
                return HandleMessageResult.InternalError;
            }

            _logger.LogDebug("ReactOnOffer event error. Requested room not exist.");
            return HandleMessageResult.InternalError;
        }

        private async Task<HandleMessageResult> HandleOfferRequest(Shared.Models.Requests.WebRTCNegotiation request, BaseMessage message)
        {
            _logger.LogTrace("ReactOnOfferRequest event initiated.");
            if (_coordinator.GetVideoSession(request.RoomId, out var session) == SessionRequestResult.NoError)
            {
                if (session!.GetPeerById(request.PeerId, out _))
                {
                    var offerSdp = new Shared.Models.Requests.WebRTCNegotiation(
                        WebRTCNegotiationType.OfferRequest,
                        request.PeerId, request.RoomId,
                        await _coordinator.ReactOnOfferRequest(request.PeerId));
                    SendResponse(offerSdp, WebRTCNegotiationResult.NoError, message);
                    return HandleMessageResult.NoError;
                }

                _logger.LogDebug("ReactOnOfferRequest event error. Requested peer not exist.");
                SendResponse(request, WebRTCNegotiationResult.InternalError, message);
                return HandleMessageResult.InternalError;
            }

            _logger.LogDebug("ReactOnOfferRequest event error. Requested room not exist.");
            SendResponse(request, WebRTCNegotiationResult.InternalError, message);
            return HandleMessageResult.InternalError;
        }

        private HandleMessageResult HandleAnswer(Shared.Models.Requests.WebRTCNegotiation request, BaseMessage message)
        {
            _logger.LogTrace("ReactOnAnswer event initiated.");
            var sdpAnswer = (RTCSessionDescriptionInit)request.Data;
            if (_coordinator.GetVideoSession(request.RoomId, out var session) == SessionRequestResult.NoError)
            {
                if (session!.GetPeerById(request.PeerId, out _))
                {
                    if (sdpAnswer != null)
                    {
                        _coordinator.ReactOnAnswer(request.PeerId, sdpAnswer);
                        _logger.LogTrace("ReactOnAnswer event completed.");
                        return HandleMessageResult.NoError;
                    }
                    
                    _logger.LogDebug("ReactOnAnswer event error. Null SDP.");
                    return HandleMessageResult.InternalError;
                }
                
                _logger.LogDebug("ReactOnAnswer event error. Requested peer not exist.");
                return HandleMessageResult.InternalError;
            }

            _logger.LogDebug("ReactOnAnswer event error. Requested room not exist.");
            return HandleMessageResult.InternalError;
        }

        private HandleMessageResult HandleICE(Shared.Models.Requests.WebRTCNegotiation request, BaseMessage message)
        {
            _logger.LogTrace("ReactOnICE event initiated.");
            var candidate = (RTCIceCandidateInit)request.Data;
            if (_coordinator.GetVideoSession(request.RoomId, out var session) == SessionRequestResult.NoError)
            {
                if (session.GetPeerById(request.PeerId, out _))
                {
                    _coordinator.ReactOnICE(request.PeerId ,candidate);
                    _logger.LogTrace("ReactOnICE event completed.");
                    return HandleMessageResult.NoError;
                }

                _logger.LogDebug("ReactOnICE event error. Requested peer not exist.");
                return HandleMessageResult.InternalError;
            }

            _logger.LogDebug("ReactOnICE event error. Requested room not exist.");
            return HandleMessageResult.InternalError;
        }

        private void SendResponse(Shared.Models.Requests.WebRTCNegotiation request, WebRTCNegotiationResult result, BaseMessage message)
        {
            var response = new Shared.Models.Requests.WebRTCNegotiation(request, result);
            message.Data = response;
            _coordinator.SendMessageToUser(message.UserId, message);
        }
    }
}

