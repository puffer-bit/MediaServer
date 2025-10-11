using SIPSorceryMedia.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Services.Other.ScreenCastService
{
    public interface IScreenCastClient : IDisposable
    {
        event Action<byte[]> FrameReceived;
        Task InitializeAsync();
        Task<bool> CreateSessionAsync();
        Task<bool> SelectSourcesAsync();
        Task<bool> StartSessionAsync();
        Task<bool> CloseSessionAsync();
    }
}
