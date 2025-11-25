using Serilog;
using SIPSorceryMedia.FFmpeg;

namespace Server.MainServer.Main.Server.Bootstrap
{
    public class InitialServerLoader
    {
        public InitialServerLoaderContext Context { get; set; }
        public bool IsFfmpegInitialized { get; set; }

        public InitialServerLoader(InitialServerLoaderContext context)
        {
            Context = context;
        }

        public void SetContext(InitialServerLoaderContext context)
        {
            Context = context;
        }
        
        public bool FfmpegInitialize()
        {
            try
            {
                if (OperatingSystem.IsWindows())
                {
                    FFmpegInit.Initialise(FfmpegLogLevelEnum.AV_LOG_ERROR);
                }
                else if (OperatingSystem.IsLinux())
                {
                    FFmpegInit.Initialise(FfmpegLogLevelEnum.AV_LOG_ERROR, AppContext.BaseDirectory);
                }

                IsFfmpegInitialized = true;
                return true;
            }
            catch (ApplicationException ex)
            {
                Log.Logger.Debug("Ffmpeg initialize exception: {exception}", ex.Message);
                return false;
            }
        }
    }
}
