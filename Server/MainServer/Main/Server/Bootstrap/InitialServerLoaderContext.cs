namespace Server.MainServer.Main.Server.Bootstrap
{
    public class InitialServerLoaderContext
    {
        public DatabaseConfig Database { get; init; } = new DatabaseConfig();
        public MainServerConfig MainServer { get; init; } = new MainServerConfig();

        public InitialServerLoaderContext()
        {

        }
    }

    public class DatabaseConfig
    {
        public string? ConnectionString { get; set; }
        public string Provider { get; set; } = "SQLite";
    }

    public class MainServerConfig
    {
        public string MainServerIpAddress { get; set; } = "localhost";
        public int Port { get; set; } = 26666;
        public int TimeOutTimeInSeconds { get; set; } = 25;
        public bool EnableFfmpeg { get; set; }
        public bool ActivateAspNetServer { get; set; }
    }
}
