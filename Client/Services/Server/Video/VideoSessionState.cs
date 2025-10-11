namespace Client.Services.Server.Video;

public enum VideoSessionState
{
    Undefined = 0,
    New = 1,
    WaitingForHost = 2,
    WaitingForNegotiation = 3,
    ReadyForNegotiation = 4,
    Connected = 5,
    Disconnected = 6,
    Failed = 7,
    TimedOut = 8,
    Ended = 9,
    WaitingForUserStream = 10,
    Canceled = 11
}