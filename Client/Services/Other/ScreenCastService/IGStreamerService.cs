using System;
using Client.Services.Other.FrameProcessor;
using Client.Services.Other.ScreenCastService.Windows.Win32PortalClient;
using SIPSorceryMedia.Abstractions;

namespace Client.Services.Other.ScreenCastService;

public interface IGStreamerService : IDisposable
{
    event Action<byte[]?> FrameReceived;
    void SetPipelineData(WindowsScreenCastType type, string id);
    bool CreatePipeline();
    bool StartPipeline();
    void PausePipeline();
    void DestroyPipeline();
}