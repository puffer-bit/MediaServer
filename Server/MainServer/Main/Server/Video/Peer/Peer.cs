using Shared.Enums;

namespace Server.MainServer.Main.Server.Video.Peer;

public class Peer : IPeer
{
    private readonly IPeerContext _context;
    private readonly ILogger<Peer> _logger;
    public bool IsStreamHost { get; set; }
    public bool IsAudioRequested { get; set; }
    public VideoSessionApproveState ApproveState { get; set; }

    public Peer(
        IPeerContext context, 
        ILogger<Peer> logger,
        bool isAudioRequested)
    {
        _context = context;
        _logger = logger;
        IsAudioRequested = isAudioRequested;
        ApproveState = VideoSessionApproveState.WaitingForApprove;
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