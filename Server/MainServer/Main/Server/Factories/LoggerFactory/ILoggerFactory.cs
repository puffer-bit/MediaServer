namespace Server.MainServer.Main.Server.Factories.LoggerFactory;

public interface ILoggerFactory
{
    ILogger Create(string componentName);
}