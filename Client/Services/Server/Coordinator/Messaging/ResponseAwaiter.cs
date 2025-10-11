using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shared.Enums;
using Shared.Models;

namespace Client.Services.Server.Coordinator.Messaging;

public class ResponseAwaiter
{
    private readonly Dictionary<string, (MessageType Type, TaskCompletionSource<BaseMessage> Tcs)> _pending = new();

    public async Task<BaseMessage> WaitForResponseAsync(string requestId, MessageType expectedType)
    {
        var tcs = new TaskCompletionSource<BaseMessage>();
        _pending.Add(requestId, (expectedType, tcs));

        var delayTask = Task.Delay(15000);
        var completedTask = await Task.WhenAny(tcs.Task, delayTask);

        if (completedTask == tcs.Task)
        {
            return tcs.Task.Result;
        }
        
        _pending.Remove(requestId);
        throw new TimeoutException($"Response for request timed out.");
    }

    public async Task<bool> HandleIncoming(BaseMessage message)
    {
        if (message?.MessageId == null) return false;
        if (message?.Type is null or MessageType.Undefined) return false;
        if (_pending.TryGetValue(message.MessageId, out var entry) && entry.Type == message.Type)
        {
            entry.Tcs.SetResult(message);
            _pending.Remove(message.MessageId);
            return true;
        }
        return false;
    }
}
