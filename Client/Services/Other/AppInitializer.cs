using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ManagedBass;
using Gst;
using SIPSorceryMedia.FFmpeg;

namespace Client.Services.Other;

public class AppInitializer
{
    private readonly List<string> _failedDependencies = new List<string>();
    public bool IsInitialized = false;
    public bool IsFailed = false;
    
    public async Task<bool> InitializeAsync()
    {
        if (IsInitialized)
            return true; // Returns true if application already initialized

        try
        {
            await System.Threading.Tasks.Task.Run(() =>
            {
                // FFMPEG
                if (OperatingSystem.IsWindows())
                {
                    FFmpegInit.Initialise(FfmpegLogLevelEnum.AV_LOG_ERROR, Directory.GetCurrentDirectory());
                }
                else if (OperatingSystem.IsLinux())
                {
                    FFmpegInit.Initialise(FfmpegLogLevelEnum.AV_LOG_FATAL, Path.Combine(AppContext.BaseDirectory, "Ffmpeg"));
                }
            });
        }
        catch (Exception)
        {
            _failedDependencies.Add("FFMPEG");
            IsFailed = true;
        }

        try
        {
            await System.Threading.Tasks.Task.Run(() =>
            {
                // GST
                if (OperatingSystem.IsWindows())
                {
                    Environment.SetEnvironmentVariable("GST_DEBUG", "5");
                    Environment.SetEnvironmentVariable("GST_DEBUG_NO_COLOR", "1");
                    Environment.SetEnvironmentVariable("GST_DEBUG_FILE", "gst_debug.log");
                    Environment.SetEnvironmentVariable("GST_PLUGIN_PATH", Path.Combine(AppContext.BaseDirectory, "lib", "gstreamer-1.0"));
                    Application.Init();
                }
                else if (OperatingSystem.IsLinux())
                {
                    Environment.SetEnvironmentVariable("GST_DEBUG", "5");
                    Environment.SetEnvironmentVariable("GST_DEBUG_NO_COLOR", "1");
                    Environment.SetEnvironmentVariable("GST_DEBUG_FILE", "gst_debug.log");
                    
                    Application.Init();
                    var registry = Gst.Registry.Get();
                    foreach (var plugin in registry.PluginList)
                    {
                        Console.WriteLine($"Plugin: {plugin.Name}, Path: {plugin.Filename}");
                    }
                }
            });
        }
        catch (Exception e)
        {
            _failedDependencies.Add("GST");
            IsFailed = true;
        }


        try
        {
            await System.Threading.Tasks.Task.Run(() =>
            {
                // BASS
                Bass.Init();
                Bass.Configure(Configuration.DeviceBufferLength, 200);
                Bass.Configure(Configuration.PlaybackBufferLength, 200);
                Bass.Configure(Configuration.UpdatePeriod, 7);
                Bass.Configure(Configuration.GlobalSampleVolume, 11);
            });
        }
        catch (Exception)
        {
            _failedDependencies.Add("BASS");
            IsFailed = true;
        }

        if (IsFailed)
        {
            return false;
        }
        IsInitialized = true;
        return true;
    }

    public void GetFailedDependencies(out string? failedDependencies)
    {
        if (IsFailed)
        {
            failedDependencies = "";
            for (int i = 0; i < _failedDependencies.Count; i++)
            {
                if (i + 1 == _failedDependencies.Count)
                    failedDependencies += _failedDependencies[i];
                else
                    failedDependencies += _failedDependencies[i] + ", ";
            }
        }
        else
        {
            failedDependencies = null;
        }
    }
}