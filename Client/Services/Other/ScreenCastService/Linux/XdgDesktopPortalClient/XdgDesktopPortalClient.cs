using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Client.Services.Other.FrameProcessor;
using Client.Services.Other.ScreenCastService.Windows.Win32PortalClient;
using SIPSorceryMedia.Abstractions;
using Tmds.DBus;

namespace Client.Services.Other.ScreenCastService.XdgDesktopPortalClient;

public class XdgDesktopPortalClient : IScreenCastClient, IDisposable
{
    private Connection _bus;
    private IScreenCast _portal;
    private string _token;
    private ObjectPath _sessionPath;
    
    // DI
    private readonly IGStreamerService _pipeWireService;
    
    public event Action<byte[]>? FrameReceived;
    
    public XdgDesktopPortalClient(
        IGStreamerService pipeWireService)
    {
        _pipeWireService = pipeWireService;
        
        _bus = new Connection(Address.Session);
        _token = "test_" + Guid.NewGuid().ToString("N");
        _portal = _bus.CreateProxy<IScreenCast>("org.freedesktop.portal.Desktop", "/org/freedesktop/portal/desktop");
    }

    public async Task InitializeAsync()
    {

    }

    public async Task<bool> CreateSessionAsync()
    {
        _bus = new Connection(Address.Session);
        _token = "test_" + Guid.NewGuid().ToString("N");
        _portal = _bus.CreateProxy<IScreenCast>("org.freedesktop.portal.Desktop", "/org/freedesktop/portal/desktop");
        await _bus.ConnectAsync();
        var options = new Dictionary<string, object>
        {
            { "handle_token", _token },
            { "session_handle_token", _token }
        };

        var requestPath = await _portal.CreateSessionAsync(options);
        var cts = new CancellationTokenSource(500);

        try
        {
            var responseTask = await WaitForResponseAsync(requestPath, cts.Token);
            if (responseTask.TryGetValue("session_handle", out var handle))
            {
                _sessionPath = new ObjectPath((string)handle);
                return true;
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Ожидание ответа отменено.");
            return false;
        }
        
        Console.WriteLine("CreateSessionAsync failed: no session_handle in response.");
        return false;
    }

    public async Task<bool> SelectSourcesAsync()
    {
        var options = new Dictionary<string, object>
        {
            { "handle_token", _token },
            { "types", (uint)1 },         // 1 = monitor
            { "cursor_mode", (uint)2 }    // 1 = embedded
        };

        var requestPath = await _portal.SelectSourcesAsync(_sessionPath, options);
        var cts = new CancellationTokenSource(500);

        try
        {
            IDictionary<string, object> response = await WaitForResponseAsync(requestPath, cts.Token);
        
            if (response.ContainsKey("sources"))
            {
                return true;
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Ожидание ответа отменено.");
            return false;
        }

        Console.WriteLine("SelectSourcesAsync failed: no sources in response.");
        return false;
    }

    public async Task<bool> StartSessionAsync()
    {
        var options = new Dictionary<string, object>
        {
            { "session_handle_token", _token }
        };

        var parentWindow = "wayland:0";

        var requestPath = await _portal.StartAsync(_sessionPath, parentWindow, options);
        try
        {
            IDictionary<string, object> response = await WaitForResponseAsync(requestPath);
            if (response.TryGetValue("streams", out var streamObj) &&
                streamObj is ValueTuple<uint, IDictionary<string, object>>[] streams &&
                streams.Length > 0)
            {
                var (nodeId, metadata) = streams[0];

                Console.WriteLine($"node_id: {nodeId}");
                foreach (var kvp in metadata)
                {
                    Console.WriteLine($"{kvp.Key}: {kvp.Value}");
                }

                // Можно добавить node_id в metadata, если нужно вернуть всё в одном словаре
                metadata["node_id"] = nodeId;

                _pipeWireService.SetPipelineData(WindowsScreenCastType.Undefined, nodeId.ToString()); // In XDG we dont need to set Window type
                if (_pipeWireService.CreatePipeline())
                {
                    if (_pipeWireService.StartPipeline())
                    {
                        _pipeWireService.FrameReceived += OnFrameReceived;
                    }
                }
            
                return true;
            }
        }
        catch (Exception)
        {
            return false;
        }

        return false;
    }

    public async Task<bool> CloseSessionAsync()
    {
        throw new NotImplementedException();
    }

    private async Task<IDictionary<string, object>> WaitForResponseAsync(
        ObjectPath requestPath,
        CancellationToken cancellationToken)
    {
        var req = _bus.CreateProxy<IRequest>("org.freedesktop.portal.Desktop", requestPath);
        var tcs = new TaskCompletionSource<IDictionary<string, object>>();

        // Привязываем отмену к TaskCompletionSource
        cancellationToken.Register(() =>
        {
            tcs.TrySetCanceled(cancellationToken);
        });

        await req.WatchResponseAsync((response) =>
        {
            Console.WriteLine($"Response code: {response.response}");

            if (response.response != 0)
            {
                Console.WriteLine("Portal request returned error.");
                tcs.TrySetCanceled();
                return;
            }

            tcs.TrySetResult(response.results);
        });

        return await tcs.Task;
    }
    
    private async Task<IDictionary<string, object>> WaitForResponseAsync(ObjectPath requestPath)
    {
        var req = _bus.CreateProxy<IRequest>("org.freedesktop.portal.Desktop", requestPath);
        var tcs = new TaskCompletionSource<IDictionary<string, object>>();

        await req.WatchResponseAsync((response) =>
        {
            Console.WriteLine($"Response code: {response.response}");

            if (response.response != 0)
            {
                Console.WriteLine("Portal request returned error.");
                tcs.TrySetCanceled();
                return;
            }

            tcs.TrySetResult(response.results);
        });

        return await tcs.Task;
    }

    private void OnFrameReceived(byte[] encodedFrame)
    {
        FrameReceived?.Invoke(encodedFrame);
    }

    public void Dispose()
    {
        _bus?.Dispose();
        _pipeWireService?.Dispose();
        GC.SuppressFinalize(this);
    }
}
