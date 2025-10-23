using System;
using Client.Services.Other.FrameProcessor;
using Client.Services.Other.ScreenCastService.Windows.Win32PortalClient;
using Shared.Enums;
using SIPSorceryMedia.Abstractions;

namespace Client.Services.Other.ScreenCastService;

public interface IGStreamerService : IDisposable
{
    event Action<byte[]> FrameReceived;
    void SetPipelineData(WindowsScreenCastType type, string id);
    ScreenCastResult CreatePipeline();
    ScreenCastResult StartPipeline();
    ScreenCastResult PausePipeline();
    ScreenCastResult DestroyPipeline();
}