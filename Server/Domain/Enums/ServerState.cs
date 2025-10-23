namespace Server.Domain.Enums
{
    public enum ServerState
    {
        Offline = 0,
        Starting = 1,
        WaitingForRemoteDB = 2,
        WaitingForDBValidation = 3,
        WaitingForFrameworksInit = 4,
        Online = 5,
        OnlineOverloaded = 7,
        Restarting = 8,
        Closing = 9,
        InMaintainceMode = 10,
        Failed = 11,
        Disabled = 12
    }
}
