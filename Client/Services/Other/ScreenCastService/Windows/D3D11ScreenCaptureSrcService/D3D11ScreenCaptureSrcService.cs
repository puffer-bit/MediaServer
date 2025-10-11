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
        private MainLoop? _mainLoop;

        public event Action<byte[]>? FrameReceived;

        public void SetPipelineData(WindowsScreenCastType type, string hwnd)
        {
            _pipelineDescription = $"d3d11screencapturesrc monitor-handle=1 ! videoconvert ! video/x-raw,format=RGB ! appsink name=mysink";
        }

        public bool CreatePipeline()
        {
            try
            {
                if (_pipelineDescription != null && _pipeline == null)
                {
                    _pipeline = Parse.Launch(_pipelineDescription) as Pipeline;
                    if (_pipeline == null)
                    {
                        Console.WriteLine("Pipeline creation failed.");
                        return false;
                    }

                    var appsinkElement = _pipeline.GetChildByName("mysink");
                    if (appsinkElement == null)
                    {
                        Console.WriteLine("Unable to find 'mysink' in pipeline.");
                        return false;
                    }

                    _appSink = new AppSink(appsinkElement.Handle);
                    _appSink.EmitSignals = true;
                    _appSink.MaxBuffers = 3;
                    _appSink.Drop = true;
                    _appSink.WaitOnEos = false;

                    _appSink.NewSample += ProceedFrameData;

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
                if (_pipeline != null)
                {
                    _pipeline.SetState(State.Playing);


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
                _pipeline?.SetState(State.Paused);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to pause pipeline. Exception: {e}");
            }
        }

        private void ProceedFrameData(object o, NewSampleArgs args)
        {
            try
            {
                using var sample = _appSink!.PullSample();
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ProceedFrameData: {ex.Message}");
            }
        }

        private void OnFrameCompleted(byte[] encodedFrame)
        {
            FrameReceived?.Invoke(encodedFrame);
        }

        public void DestroyPipeline()
        {
            try
            {
                _pipeline?.SetState(State.Null);

                _pipeline?.Dispose();
                _pipeline = null;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to destroy pipeline. Exception: {e}");
            }
        }

        public void Dispose()
        {
            DestroyPipeline();
            _appSink?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
