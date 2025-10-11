using System;
using System.Threading.Tasks;
using Shared.Enums;
using Shared.Models;
using Shared.Models.Requests;
using Shared.Models.Requests.SessionInfo;

namespace Client.Services.Server.Coordinator;

public partial class CoordinatorSession
{
    public void SendMessage(BaseMessage message)
    {
        _messageSender.SendMessage(message);
    }

    public async Task ProcessEvent(BaseMessage message)
    {
        if (!await _responseAwaiter.HandleIncoming(message))
        {
            switch (message.Type)
            {
                case MessageType.Heartbeat:
                    _heartbeatHandler.ReactOnPing();
                    break;
                case MessageType.WebRTCInit:
                    if (message.Data is WebRTCNegotiation request)
                    {
                        if (request.Type == WebRTCNegotiationType.ICE)
                        {
                            _sessionsManager.ReactOnICE(request);
                        }
                    }
                    else
                        Console.WriteLine("WebRTCInit error. Failed to parse message.");
                    break;
                case MessageType.SessionsStateChanged:
                    if (message.Data is SessionStateChanged stateChangedMessage)
                    {
                        _sessionsManager.HandleSessionStateChanged(stateChangedMessage);
                    }
                    else
                        Console.WriteLine("SessionsStateChanged error. Failed to parse message.");
                    break;
            }
        }
    }
}