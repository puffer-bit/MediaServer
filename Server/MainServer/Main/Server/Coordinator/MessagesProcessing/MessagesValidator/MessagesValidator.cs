using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared.Enums;
using Shared.Models;
using Shared.Models.Requests;
using Shared.Models.Requests.SessionInfo;
using SIPSorcery.Net;

namespace Server.MainServer.Main.Server.Coordinator.MessagesProcessing.MessagesValidator
{
    public class MessagesValidator(ILogger<MessagesValidator> logger)
    {
        public bool Validate(BaseMessage message)
        {
            try
            {
                ValidateMessageResult result = message.Type switch
                {
                    MessageType.WebRTCInit => ValidateFields(((JObject)message.Data).ToObject<WebRTCNegotiation>()),
                    MessageType.RoomRequest => ValidateFields(((JObject)message.Data).ToObject<UserSessionRequestModel>()),
                    _ => ValidateMessageResult.ForbiddenData
                };

                if (result == ValidateMessageResult.NoError)
                {
                    return true;
                }

                return false;
            }
            catch (JsonException jsonE)
            {
                logger.LogDebug("Message identify error. Json parse failed. Exception: {Field}", jsonE.Message);
                return false;
            }
        }
        
        private ValidateMessageResult ValidateFields(UserSessionsInfoRequestModel? request)
        {
            ValidateMessageResult LogAndReturn(string field, ValidateMessageResult result)
            {
                var label = result switch
                {
                    ValidateMessageResult.NullDataReceived => "Null",
                    ValidateMessageResult.ForbiddenData => "Forbidden",
                    _ => "Unknown"
                };

                logger.LogDebug("Room request verifying error. {Label} data received ({Field}).", label, field);
                return result;
            }
            
            try
            {
                if (request == null)
                    return LogAndReturn("Entire message", ValidateMessageResult.NullDataReceived);

                if (request.RequestType == null)
                    return LogAndReturn("Type", ValidateMessageResult.NullDataReceived);

                if (!Enum.IsDefined(typeof(SessionsRequestType), request.RequestType))
                    return LogAndReturn("Type", ValidateMessageResult.ForbiddenData);

                switch (request.RequestType)
                {
                    case SessionsRequestType.Create:
                        if (string.IsNullOrWhiteSpace(request.Session.Name))
                            return LogAndReturn("RoomName", ValidateMessageResult.NullDataReceived);

                        if (request.Session.Capacity <= 1)
                            return LogAndReturn("Capacity", ValidateMessageResult.ForbiddenData);
                        break;

                    case SessionsRequestType.Join:
                        if (request.RoomId == null)
                            return LogAndReturn("RoomID", ValidateMessageResult.NullDataReceived);
                        break;

                    default:
                        if (request.RoomId == null)
                            return LogAndReturn("RoomID", ValidateMessageResult.NullDataReceived);

                        if (request.PeerId == null)
                            return LogAndReturn("PeerID", ValidateMessageResult.NullDataReceived);
                        break;
                }
                
                return ValidateMessageResult.NoError;
            }
            catch (Exception e)
            {
                logger.LogDebug("Room request verifying error. Exception: {Field}", e.Message);
                return ValidateMessageResult.NotExceptedError;
            }
        }

        private ValidateMessageResult ValidateFields(WebRTCNegotiation? request)
        {
                ValidateMessageResult LogAndReturn(string field, ValidateMessageResult result)
            {
                var label = result switch
                {
                    ValidateMessageResult.NullDataReceived => "Null",
                    ValidateMessageResult.ForbiddenData => "Forbidden",
                    _ => "Unknown"
                };

                logger.LogDebug("WebRTC negotiation request verifying error. {Label} data received ({Field}).", label, field);
                return result;
            }

            try
            {
                if (request == null)
                    return LogAndReturn("Entire message", ValidateMessageResult.NullDataReceived);

                if (request.PeerId == null)
                    return LogAndReturn("PeerID", ValidateMessageResult.NullDataReceived);

                if (request.RoomId == null)
                    return LogAndReturn("RoomID", ValidateMessageResult.NullDataReceived);

                if (!Enum.IsDefined(typeof(WebRTCNegotiationType), request.Type))
                    return LogAndReturn("Type", ValidateMessageResult.ForbiddenData);

                switch (request.Type)
                {
                    case WebRTCNegotiationType.Answer:
                    case WebRTCNegotiationType.Offer:
                        if (request.Data == null)
                            return LogAndReturn("Data", ValidateMessageResult.NullDataReceived);
                        RTCSessionDescriptionInit? description = ((JObject)request.Data!).ToObject<RTCSessionDescriptionInit>();
                        break;
                    
                    case WebRTCNegotiationType.ICE:
                        if (request.Data == null)
                            return LogAndReturn("Data", ValidateMessageResult.NullDataReceived);
                        var candidate = ((JObject)request.Data!).ToObject<RTCIceCandidateInit>();
                        break;
                    
                    case WebRTCNegotiationType.OfferRequest:
                        break;
                        
                    default:
                        return LogAndReturn("Type", ValidateMessageResult.ForbiddenData);
                }

                return ValidateMessageResult.NoError;
            }
            catch (InvalidCastException castE)
            {
                logger.LogDebug("WebRTC negotiation verifying error. Json exception: {Field}", castE.Message);
                return ValidateMessageResult.JsonParseError;
            }
            catch (Exception e)
            {
                logger.LogDebug("WebRTC negotiation verifying error. Exception: {Field}", e.Message);
                return ValidateMessageResult.NotExceptedError;
            }
        }
    }
}

