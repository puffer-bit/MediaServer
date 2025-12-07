using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Client.Services.HardwareIdentifier.GPU;

public class GpuInfo
{
    public string Name { get; set; }
    public string Vendor { get; set; }
    
    public static List<GpuInfo> GetGpuInfo()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return GetWindowsGpuInfo();
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return GetLinuxGpuInfo();
        else
            return new List<GpuInfo> { new GpuInfo { Name = "Unknown", Vendor = "Unknown" } };
    }
    
    public static string DetectGpuVendor(string gpuName)
    {
        if (gpuName.Contains("NVIDIA", StringComparison.OrdinalIgnoreCase))
            return "NVIDIA";
        if (gpuName.Contains("AMD", StringComparison.OrdinalIgnoreCase) || gpuName.Contains("ATI", StringComparison.OrdinalIgnoreCase))
            return "AMD";
        if (gpuName.Contains("Intel", StringComparison.OrdinalIgnoreCase))
            return "Intel";
        return "Unknown";
    }
    
    public static bool HasHardwareAcceleration()
    {
        var hasNvenc = Gst.ElementFactory.Find("nvh264enc") != null;
        var hasVaapi = Gst.ElementFactory.Find("vaapih264enc") != null;

        return hasNvenc || hasVaapi;
    }

    private static List<GpuInfo> GetWindowsGpuInfo()
    {
        var gpus = new List<GpuInfo>();
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = "Get-WmiObject Win32_VideoController | Select-Object Name, AdapterCompatibility",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            foreach (var line in output.Split('\n'))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Trim().Split(new[] { "  " }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    gpus.Add(new GpuInfo { Name = parts[0].Trim(), Vendor = parts[1].Trim()});
                }
            }
        }
        catch (Exception ex)
        {
            gpus.Add(new GpuInfo { Name = "Error", Vendor = ex.Message});
        }
        return gpus;
    }

    private static List<GpuInfo> GetLinuxGpuInfo()
    {
        var gpus = new List<GpuInfo>();
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "bash",
                    Arguments = "-c \"lspci | grep -i vga\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            foreach (var line in output.Split('\n'))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                gpus.Add(new GpuInfo { Name = line.Trim(), Vendor = line.Contains("NVIDIA") ? "NVIDIA" :
                                                           line.Contains("AMD") ? "AMD" :
                                                           line.Contains("Intel") ? "Intel" : "Unknown" });
            }
        }
        catch (Exception ex)
        {
            gpus.Add(new GpuInfo { Name = "Error", Vendor = ex.Message });
        }
        return gpus;
    }
}