using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Client.Views.Sessions.VideoSessionView;
using SIPSorceryMedia.Abstractions;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Client.Services.Other.ScreenCastService.Windows.Win32PortalClient
{
    internal class Win32PortalClient : IScreenCastClient
    {
        private WindowsScreenCastType? _castType;
        private (WindowsScreenCastType, string)? _selectedSource;
        public event Action<byte[]>? FrameReceived;

        // DI
        private readonly IGStreamerService _d3d11ScreenCaptureSrcService;

        public Win32PortalClient(IGStreamerService d3d11ScreenCaptureSrcService)
        {
            _d3d11ScreenCaptureSrcService = d3d11ScreenCaptureSrcService;
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
            var lifetime = Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            var mainWindow = lifetime?.MainWindow;

            var dialog = new VideoSourceSelectWindow();
            _selectedSource = await dialog.ShowDialog<(WindowsScreenCastType, string)>(mainWindow!);
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
            _d3d11ScreenCaptureSrcService.SetPipelineData(type, id);
            _d3d11ScreenCaptureSrcService.CreatePipeline();
            _d3d11ScreenCaptureSrcService.StartPipeline();
            _d3d11ScreenCaptureSrcService.FrameReceived += OnFrameReceived;

            return true;
        }

        public async Task<bool> CloseSessionAsync()
        {
            throw new NotImplementedException();
        }

        private void OnFrameReceived(byte[] encodedFrame)
        {
            FrameReceived?.Invoke(encodedFrame);
        }

        public void Dispose()
        {
            _d3d11ScreenCaptureSrcService?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
