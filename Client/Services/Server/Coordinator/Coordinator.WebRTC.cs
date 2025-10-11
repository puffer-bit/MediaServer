using System.Threading.Tasks;
using Shared.Enums;
using Shared.Models;
using Shared.Models.Requests;
using SIPSorcery.Net;

namespace Client.Services.Server.Coordinator;

public partial class CoordinatorSession
{
    public async Task<RTCSessionDescriptionInit?> RequestOffer(WebRTCNegotiation request)
    {
        var message = new BaseMessage(GetUser().Id, MessageType.WebRTCInit, request);
        SendMessage(message);
        
        var response = await _responseAwaiter.WaitForResponseAsync(message.MessageId, message.Type);
        var requestAnswer = (WebRTCNegotiation)response.Data;
        if (requestAnswer.Result == WebRTCNegotiationResult.NoError)
        {
            if (requestAnswer.Data != null && requestAnswer.Data is RTCSessionDescriptionInit sdpAnswer)
            {
                return sdpAnswer;
            }
        }

        return null;
    }

    public void SendICE(WebRTCNegotiation request)
    {
        var message = new BaseMessage(GetUser().Id, MessageType.WebRTCInit, request);
        SendMessage(message);
    }

    public void SendAnswer(WebRTCNegotiation answer)
    {
        var message = new BaseMessage(GetUser().Id, MessageType.WebRTCInit, answer);
        SendMessage(message);
    }
}