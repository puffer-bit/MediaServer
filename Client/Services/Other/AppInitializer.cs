using System;
using System.Collections.Generic;
using System.IO;
using Gst;
using ManagedBass;
using SIPSorceryMedia.FFmpeg;

namespace Client.Services.Other;

public class AppInitializer
{
    private readonly List<string> _failedDependencies = new List<string>();
    public bool IsInitialized = false;
    public bool IsFailed = false;
    
    public bool Initialize()
    {
        if (IsInitialized)
            return true; // Returns true if application already initialized

        try
        {
            // FFMPEG
            if (OperatingSystem.IsWindows())
            {
                FFmpegInit.Initialise(FfmpegLogLevelEnum.AV_LOG_ERROR, Directory.GetCurrentDirectory());
            }
            else if (OperatingSystem.IsLinux())
            {
                FFmpegInit.Initialise(FfmpegLogLevelEnum.AV_LOG_FATAL, Directory.GetCurrentDirectory());
            }
        }
        catch (Exception)
        {
            _failedDependencies.Add("FFMPEG");
            IsFailed = true;
        }

        try
        {
            Environment.SetEnvironmentVariable("GST_DEBUG", "5");
            Environment.SetEnvironmentVariable("GST_DEBUG_NO_COLOR", "1");
            Environment.SetEnvironmentVariable("GST_DEBUG_FILE", "gst_debug.log");
            // GST
            Application.Init();
        }
        catch (Exception)
        {
            _failedDependencies.Add("GST");
            IsFailed = true;
        }


        try
        {
            // BASS
            Bass.Init();
            Bass.Configure(Configuration.DeviceBufferLength, 200);
            Bass.Configure(Configuration.PlaybackBufferLength, 200);
            Bass.Configure(Configuration.UpdatePeriod, 7);
            Bass.Configure(Configuration.GlobalSampleVolume, 11);
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