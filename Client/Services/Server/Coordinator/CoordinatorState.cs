namespace Client.Services.Server.Coordinator;

public enum CoordinatorState
{
    New = 0,
    Connecting = 1,
    Reconnecting = 11,
    AuthenticationNeeded = 2,
    InternalServerError = 3,
    TimedOut = 4,
    HeartbeatTimedOut = 5,
    AuthenticationFailed = 12,
    Failed = 6,
    Connected = 7,
    Disconnected = 8,
    Denied = 9
}