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
        public string Provider { get; set; } = "sqlite";
    }

    public class MainServerConfig
    {
        public string IpAddress { get; set; }
        public ushort Port { get; set; }
        public int TimeOutTimeInSeconds { get; set; }
        public bool EnableFfmpeg { get; set; }
        public bool ActivateWebServer { get; set; }
        
        public bool EnableTurn { get; set; }
        public bool EnableStun { get; set; }

        public string? TurnAddress { get; set; }
        public ushort TurnPort { get; set; }
        public string TurnUsername { get; set; }
        public string TurnPassword { get; set; }
        
        public string? StunAddress { get; set; }
        public ushort StunPort { get; set; }
    }
}
