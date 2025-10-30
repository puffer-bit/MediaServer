using Shared.Models;
using Shared.Models.DTO;

namespace Server.MainServer.Main.Server.Coordinator.Users.Manager;

public class UserManager : IUserManager
{
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

    public bool AddUser(UserDTO user, out UserDTO? addedUser)
    {
        if (user.Id == null)
        {
            user.Id = Guid.NewGuid().ToString();
        }
        if (_context.ConnectedUsers.ContainsKey(user.Id))
        {
            _logger.LogWarning($"User with id {user.Id} already connected. Replacing...");
            _context.ConnectedUsers[user.Id] = user;
        }
        else
        {
            _context.ConnectedUsers.Add(user.Id, user);
        }

        addedUser = user;
        return true;
    }
        
    public void RemoveUser(string userId)
    {
        _context.ConnectedUsers.Remove(userId);
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
}