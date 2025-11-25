namespace Server.MainServer.Main.Server.Coordinator.Users.Authenticator;

public class Authenticator : IAuthenticator
{
    private readonly ICoordinatorInstance _coordinatorInstance;
    
    public Authenticator(ICoordinatorInstance coordinatorInstance)
    {
        _coordinatorInstance = coordinatorInstance;
    }

    public bool CheckUserIdentity()
    {
        throw new NotImplementedException();
    }
}