using Shared.Enums;

namespace Server.MainServer.Main.Server.Coordinator.Sessions.Video.Peer;

public class Peer : IPeer
{
    private readonly IPeerContext _context;
    private readonly ILogger<Peer> _logger;
    public bool IsStreamHost { get; set; }
    public bool IsAudioRequested { get; set; }
    public VideoSessionPeerState PeerState { get; set; }
    public bool IsNegotiated { get; set; }
    public bool IsConnected { get; set; }
    public bool IsApproved { get; set; }
    public bool IsRejected { get; set; }

    public Peer(
        IPeerContext context, 
        ILogger<Peer> logger,
        bool isAudioRequested)
    {
        _context = context;
        _logger = logger;
        IsAudioRequested = isAudioRequested;
        PeerState = VideoSessionPeerState.WaitingForApprove;
    }

    public void MakeHost()
    {
        IsStreamHost = true;
    }

    public string GetId()
    {
        return _context.Id;
    }

    public string GetUserId()
    {
        return _context.UserId;
    }
}