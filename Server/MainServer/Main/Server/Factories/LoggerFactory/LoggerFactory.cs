using Serilog;
using Serilog.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Server.MainServer.Main.Server.Factories.LoggerFactory;

public class LoggerFactory : ILoggerFactory
{
    private readonly SerilogLoggerFactory _serilogFactory;

    public LoggerFactory()
    {
        _serilogFactory = new SerilogLoggerFactory(Log.Logger);
    }

    public ILogger Create(string componentName)
    {
        var serilogLogger = Log.ForContext("Component", componentName);
        return new SerilogLoggerFactory(serilogLogger).CreateLogger(componentName);
    }
}