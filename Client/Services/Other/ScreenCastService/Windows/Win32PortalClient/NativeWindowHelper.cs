using Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static Client.Services.Other.ScreenCastService.Windows.Win32PortalClient.NativeWindowHelper;

namespace Client.Services.Other.ScreenCastService.Windows.Win32PortalClient
{
    public class NativeWindowHelper
    {
        [DllImport("user32.dll")]
        static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        [DllImport("user32.dll")]
        static extern bool IsWindowVisible(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);
        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        public static extern bool EnumDisplayDevices(
            string lpDevice,
            uint iDevNum,
            ref DISPLAY_DEVICE lpDisplayDevice,
            uint dwFlags);
        private delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);
        delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        private const int MONITORINFOF_PRIMARY = 0x00000001;

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct MONITORINFOEX
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public int dwFlags;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szDevice;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct DISPLAY_DEVICE
        {
            public int cb;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;
            public int StateFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceKey;
        }


        public class WindowData
        {
            public required string Title { get; set; }
            public IntPtr Handle { get; set; }
        }

        public class MonitorData
        {
            public required string Title { get; set; }
            public IntPtr Handle { get; set; }
            public RECT MonitorArea { get; set; }
            public RECT WorkArea { get; set; }
            public bool IsPrimary { get; set; }
        }

        public static List<WindowData> GetOpenWindows()
        {
            var windows = new List<WindowData>();

            EnumWindows((hWnd, lParam) =>
            {
                if (IsWindowVisible(hWnd))
                {
                    StringBuilder sb = new StringBuilder(256);
                    GetWindowText(hWnd, sb, sb.Capacity);
                    string title = sb.ToString();
                    if (!string.IsNullOrWhiteSpace(title))
                        windows.Add(new WindowData()
                        {
                            Title = title,
                            Handle = hWnd
                        });
                }
                return true;
            }, IntPtr.Zero);
            return windows;
        }

        public static List<MonitorData> GetMonitors()
        {
            var monitors = new List<MonitorData>();

            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
            (IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData) =>
            {
                var info = new MONITORINFOEX();
                info.cbSize = Marshal.SizeOf(info);

                if (GetMonitorInfo(hMonitor, ref info))
                {
                    string modelName = info.szDevice; // fallback

                    try
                    {
                        using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DesktopMonitor");
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            var deviceId = obj["PNPDeviceID"]?.ToString();
                            var name = obj["Name"]?.ToString();

                            if (!string.IsNullOrWhiteSpace(deviceId) && info.szDevice.Contains(deviceId.Substring(0, 8)))
                            {
                                modelName = name;
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("WMI error: " + ex.Message);
                    }

                    monitors.Add(new MonitorData
                    {
                        Title = modelName,
                        Handle = hMonitor,
                        MonitorArea = info.rcMonitor,
                        WorkArea = info.rcWork,
                        IsPrimary = (info.dwFlags & MONITORINFOF_PRIMARY) != 0
                    });
                }

                return true;
            }, IntPtr.Zero);


            return monitors;
        }
    }
}
