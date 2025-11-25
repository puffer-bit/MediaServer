using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Client.Services.Other.ScreenCastService.Windows.Win32PortalClient;
using Gst;
using Gst.App;
using Shared.Enums;
using SIPSorceryMedia.Abstractions;

namespace Client.Services.Other.ScreenCastService.Linux.PipeWireService;

public class PipeWireService : IGStreamerService, IDisposable
{
    private AppSink? _appSink;
    private Pipeline? _pipeline;
    private string? _pipelineDescription;
    
    public event Action<byte[]>? FrameReceived;

    public PipeWireService()
    {

    }

    public void SetPipelineData(WindowsScreenCastType _, string nodeId)
    {
        _pipelineDescription = 
            $"pipewiresrc path={nodeId} ! videoconvert ! video/x-raw,format=NV12 ! nvh264enc bitrate=4000 preset=low-latency-hq ! h264parse config-interval=1 ! appsink name=mysink";    
    }

    public ScreenCastResult CreatePipeline()
    {
        try
        {
            if (_pipelineDescription != null && _pipeline == null)
            {
                _pipeline = Parse.Launch(_pipelineDescription) as Pipeline;

                // Try to get the appsink by name
                var appsinkElement = _pipeline!.GetChildByName("mysink");
                if (appsinkElement == null)
                {
                    Console.WriteLine("Unable to find 'mysink' in pipeline.");
                    return ScreenCastResult.InternalError;
                }

                _appSink = new AppSink(appsinkElement.Handle);
                _appSink.EmitSignals = true;
                _appSink.MaxBuffers = 3;
                _appSink.Drop = false;
                _appSink.WaitOnEos = false;
                appsinkElement.Dispose();
                return ScreenCastResult.NoError;
            }
            return ScreenCastResult.NotInitialized;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to build pipeline. Exception: {e}");
            return ScreenCastResult.InternalError;
        }
    }

    public ScreenCastResult StartPipeline()
    {
        try
        {
            if (_pipeline != null && _appSink != null)
            {
                _pipeline.SetState(State.Playing);
                _appSink.NewSample += ProceedFrameData;

                return ScreenCastResult.NoError;
            }
            return ScreenCastResult.NotInitialized;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to start pipeline. Exception: {e}");
            return ScreenCastResult.InternalError;
        }
    }

    public ScreenCastResult PausePipeline()
    {
        try
        {
            if (_pipeline != null && _appSink != null)
            {
                _appSink.NewSample -= ProceedFrameData;
                _pipeline.SetState(State.Paused);
                return ScreenCastResult.NoError;
            }
            return ScreenCastResult.NotInitialized;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to pause pipeline. Exception: {e}");
            return ScreenCastResult.InternalError;
        }
    }

    private void ProceedFrameData(object o, NewSampleArgs args)
    {
        using var sample = _appSink!.TryPullSample(100);
        if (sample != null)
        {
            using var buffer = sample.Buffer;
            buffer.Map(out MapInfo map, MapFlags.Read);

            byte[] encodedFrame = new byte[map.Size];
            Marshal.Copy(map.DataPtr, encodedFrame, 0, (int)map.Size);
            OnFrameCompleted(encodedFrame);
            buffer.Unmap(map);
        }
    }

    private void OnFrameCompleted(byte[] encodedFrame)
    {
        FrameReceived?.Invoke(encodedFrame);
    }

    public ScreenCastResult DestroyPipeline()
    {
        PausePipeline();
        
        _appSink?.Dispose();
        _pipeline?.Dispose();
        _pipeline = null;
        return ScreenCastResult.NoError;
    }

    public void Dispose()
    {
        PausePipeline();
        
        _appSink?.Dispose();
        _pipeline?.Dispose();
        GC.SuppressFinalize(this);
    }
}