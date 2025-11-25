using Server.MainServer.Main.Server.Factories.VideoSessionFactory;

namespace Server.MainServer.Main.Server.Coordinator.Sessions.Manager;

public partial class SessionManager : ISessionManager
{
    private readonly ILogger _logger;
    private readonly IVideoSessionFactory _videoSessionFactory;
    private readonly ISessionManagerContext _context;
    private readonly CoordinatorInstance _coordinator;

    public SessionManager(ILoggerFactory loggerFactory, 
        ISessionManagerContext context,
        IVideoSessionFactory videoSessionFactory, 
        CoordinatorInstance coordinator)
    {
        _logger = loggerFactory.CreateLogger("SessionManager");    
        _context = context;
        _videoSessionFactory = videoSessionFactory;
        _coordinator = coordinator;
    }

    public void CloseAllSessions()
    {
        foreach (var videoSession in _context.VideoSessions)
        {
            _coordinator.SessionDeleted(videoSession.Value.GetSessionId());
            videoSession.Value.CloseSession();
        }
        _context.VideoSessions.Clear();
    }
}