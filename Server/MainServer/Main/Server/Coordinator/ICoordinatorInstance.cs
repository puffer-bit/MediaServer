namespace Server.MainServer.Main.Server.Coordinator;

public interface ICoordinatorInstance
{
    ICoordinatorInstanceContext Context { get; init; }
}