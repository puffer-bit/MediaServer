using SIPSorceryMedia.Abstractions;
using System;
using Gst;
using Gst.App;
using System.Runtime.InteropServices;
using GLib;
using System.Threading;
using Client.Services.Other.ScreenCastService.Windows.Win32PortalClient;

namespace Client.Services.Other.ScreenCastService.Windows.D3D11ScreenCaptureSrcService
{
    public class D3D11ScreenCaptureSrcService : IGStreamerService
    {
        private AppSink? _appSink;
        private Pipeline? _pipeline;
        private string? _pipelineDescription;

        public event Action<byte[]>? FrameReceived;

        public D3D11ScreenCaptureSrcService()
        {

        }

        public void SetPipelineData(WindowsScreenCastType castType, string nodeId)
        {
            if (castType == WindowsScreenCastType.Window)
            {
                _pipelineDescription =
               $"d3d11screencapturesrc window-handle={nodeId} show-cursor=true ! videoconvert ! video/x-raw,format=NV12 ! nvh264enc bitrate=4000 preset=low-latency-hq ! h264parse config-interval=1 ! appsink name=mysink";
            }
            else if (castType == WindowsScreenCastType.Monitor)
            {
                _pipelineDescription =
               $"d3d11screencapturesrc monitor-handle={nodeId} show-cursor=true ! videoconvert ! video/x-raw,format=NV12 ! nvh264enc bitrate=4000 preset=low-latency-hq ! h264parse config-interval=1 ! appsink name=mysink";
            }
        }

        public bool CreatePipeline()
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
                        return false;
                    }

                    _appSink = new AppSink(appsinkElement.Handle);
                    _appSink.EmitSignals = true;
                    _appSink.MaxBuffers = 3;
                    _appSink.Drop = false;
                    appsinkElement.Dispose();
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to build pipeline. Exception: {e}");
                return false;
            }
        }

        public bool StartPipeline()
        {
            try
            {
                if (_pipeline != null && _appSink != null)
                {
                    _pipeline.SetState(State.Playing);
                    _appSink.NewSample += ProceedFrameData;

                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to start pipeline. Exception: {e}");
                return false;
            }
        }

        public void PausePipeline()
        {
            try
            {
                if (_pipeline != null && _appSink != null)
                {
                    _appSink.NewSample -= ProceedFrameData;
                    _pipeline.SetState(State.Paused);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to pause pipeline. Exception: {e}");
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

        public void DestroyPipeline()
        {
            _pipeline?.Dispose();
            _pipeline = null;
        }

        public void Dispose()
        {
            _appSink?.Dispose();
            _pipeline?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
