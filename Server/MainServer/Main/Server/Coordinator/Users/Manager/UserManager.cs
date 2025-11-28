using Shared.Models;
using Shared.Models.DTO;

namespace Server.MainServer.Main.Server.Coordinator.Users.Manager;

public class UserManager : IUserManager
{
    public int ConnectedUsersCount => _context.ConnectedUsers.Count;
    
    private readonly IUserManagerContext _context;
    private readonly ILogger _logger;
    private readonly CoordinatorInstance _coordinator;

    public UserManager(IUserManagerContext context, 
        ILoggerFactory loggerFactory, 
        CoordinatorInstance coordinator)
    {
        _context = context;
        _logger = loggerFactory.CreateLogger("UserManager");
        _coordinator = coordinator;
    }

    public bool AddUser(UserDTO user, out UserDTO addedUser)
    {
        if (user.Id == null)
        {
            user.Id = Guid.NewGuid().ToString();
        }
        if (!_context.ConnectedUsers.TryAdd(user.Id, user))
        {
            _logger.LogTrace("User with id {Id} already connected. Replacing...", user.Id);
            _context.ConnectedUsers[user.Id] = user;
        }

        if (user.Id != "system")
            _logger.LogTrace("User with id {Id} connected.", user.Id);
        
        addedUser = user;
        return true;
    }
        
    public void RemoveUser(string userId)
    {
        if (_context.ConnectedUsers.Remove(userId, out var user))
        {
            _logger.LogTrace("User with id {Id} disconnected.", user.Id);
        }
    }

    public bool GetUser(string userId, out UserDTO? user)
    {
        if (_context.ConnectedUsers.TryGetValue(userId, out var foundedUser))
        {
            user = foundedUser;
            return true;
        }
        user = null;
        return false;
    }

    public void RemoveAllUsers()
    {
        _context.ConnectedUsers.Clear();
    }
}