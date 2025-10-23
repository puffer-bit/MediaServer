using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Client.ViewModels.MessageBox;
using Client.Views.MessageBox;
using Client.Views.Sessions.VideoSessionView;
using System;
using System.Threading.Tasks;

namespace Client.Services.Other.ScreenCastService.Windows.Win32PortalClient
{
    internal class WindowsPortalClient : IScreenCastClient
    {
        private (WindowsScreenCastType, string)? _selectedSource;
        private Window _mainWindow;

        public event Action<byte[]>? FrameReceived;

        // DI
        private readonly IGStreamerService _windowsScreenCaptureService;

        public WindowsPortalClient(IGStreamerService d3d11ScreenCaptureSrcService)
        {
            _windowsScreenCaptureService = d3d11ScreenCaptureSrcService;
            var lifetime = Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            _mainWindow = lifetime!.MainWindow!;
        }

        /// <summary>
        ///  Redundant in this implementation
        /// </summary>
        /// <returns></returns>
        public async Task InitializeAsync()
        {
            return;
        }

        /// <summary>
        ///  Redundant in this implementation
        /// </summary>
        /// <returns></returns>
        public async Task<bool> CreateSessionAsync()
        {
            return true;
        }

        public async Task<bool> SelectSourcesAsync()
        {
            var dialog = new VideoSourceSelectWindow();
            _selectedSource = await dialog.ShowDialog<(WindowsScreenCastType, string)>(_mainWindow);
            if (_selectedSource == null)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> StartSessionAsync()
        {
            if (_selectedSource == null)
            {
                return false;
            }

            var (type, id) = _selectedSource.Value;
            _windowsScreenCaptureService.SetPipelineData(type, id);
            var createResult = _windowsScreenCaptureService.CreatePipeline();
            if (createResult != Shared.Enums.ScreenCastResult.NoError)
            {
                MessageBoxWindow messageBox = new MessageBoxWindow()
                {
                    DataContext = new MessageBoxViewModel(Icon.Error, Buttons.Ok,
                    $"Pipeline failed to create. Logs may contains additional information.\n\nCode: {(int)createResult}",
                    "GStreamer Error")
                };
                await messageBox.ShowDialog(_mainWindow);
                return false;
            }

            var startResult = _windowsScreenCaptureService.StartPipeline();
            if (startResult != Shared.Enums.ScreenCastResult.NoError)
            {
                MessageBoxWindow messageBox = new MessageBoxWindow()
                {
                    DataContext = new MessageBoxViewModel(Icon.Error, Buttons.Ok, 
                    $"Pipeline failed to start. Logs may contains additional information.\n\nCode: {(int)startResult}", 
                    "GStreamer Error")
                };
                await messageBox.ShowDialog(_mainWindow);
                return false;
            }
            _windowsScreenCaptureService.FrameReceived += OnFrameReceived;

            return true;
        }

        public async Task<bool> CloseSessionAsync()
        {
            var result = _windowsScreenCaptureService.DestroyPipeline();
            if (result != Shared.Enums.ScreenCastResult.NoError)
            {
                MessageBoxWindow messageBox = new MessageBoxWindow()
                {
                    DataContext = new MessageBoxViewModel(Icon.Error, Buttons.Ok,
                    $"Pipeline failed to close. Logs may contains additional information.\n\nCode: {(int)result}",
                    "GStreamer Error")
                };
                await messageBox.ShowDialog(_mainWindow);
                return false;
            }
            return true;
        }

        private void OnFrameReceived(byte[] encodedFrame)
        {
            FrameReceived?.Invoke(encodedFrame);
        }

        public void Dispose()
        {
            _windowsScreenCaptureService?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
