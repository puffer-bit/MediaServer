namespace Server.MainServer.Server_Options;

public class ServerConfigurator
{
    public DatabaseConfig Database { get; init; } = new DatabaseConfig();
    public MainServerConfig MainServer { get; init; } = new MainServerConfig();
    public ServerConfigurator()
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
    public string MainServerIpAddress { get; set; } = "localhost:26666";
    public int TimeOutTimeInSeconds { get; set; } = 25;
    public bool ActivateTestSessions { get; set; }
    public bool ActivateAspNetServer { get; set; }
}