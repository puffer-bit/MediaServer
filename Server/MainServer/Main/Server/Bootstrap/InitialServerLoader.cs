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
            if (!Context.MainServer.EnableFfmpeg)
            {
                IsFfmpegInitialized = false;
                return false;
            }
            
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
            catch (Exception ex)
            {
                Console.WriteLine("Ffmpeg initialize exception: {0}", ex);
                Console.WriteLine("Ffmpeg initialize folder: {0}", AppContext.BaseDirectory);
                IsFfmpegInitialized = false;
                return false;
            }
        }
    }
}
