using System.Diagnostics.CodeAnalysis;
using Server.MainServer.Main.Server.Coordinator.Sessions.Video.Peer;
using Server.MainServer.Main.Server.Coordinator.Sessions.Video.PeerManager;
using Server.MainServer.Main.Server.Factories.VideoSessionFactory;
using Shared.Enums;
using Shared.Models;
using Shared.Models.DTO;

namespace Server.MainServer.Main.Server.Coordinator.Sessions.Video;

public class VideoSession : IVideoSession
{
    public required Mutex Mutex { get; set; }
    private List<UserDTO> AllowedUsers { get; set; } = []; 
    public bool IsAudioRequested { get; set; }
    public bool IsHostMustApprove { get; set; }
    public bool IsHostConnected { get; set; }

    public SessionType Type => SessionType.Video;

    private readonly ILogger _logger;
    private readonly IVideoSessionContext _context;
    private readonly IVideoPeerManager _videoPeerManager;
    private readonly CoordinatorInstance _coordinator;
        
    [SetsRequiredMembers]
    public VideoSession (
        IVideoSessionContext context, 
        IVideoSessionFactory videoSessionFactory,
        CoordinatorInstance coordinator,
        bool isAudioRequested,
        bool isHostMustApprove,
        ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger($"Session({context.Id})");
        _context = context;
        _coordinator = coordinator;
        IsAudioRequested = isAudioRequested;
        IsHostMustApprove = isHostMustApprove;
        Mutex = new Mutex(false);
        
        _videoPeerManager = videoSessionFactory.CreatePeerManager(coordinator, this);
    }
        
    /// <summary>
    /// Convert Sessions to SessionDTO
    /// </summary>
    /// <returns> RoomModel </returns>
    public VideoSessionDTO AsModel() =>
        new VideoSessionDTO()
        {
            CoordinatorInstanceId = _coordinator.Context.Id,
            Id = _context.Id,
            Name = _context.Name,
            Capacity = _context.Capacity,
            HostId = _context.HostId,
            HostPeerId = _context.HostPeerId,
            IsAudioRequested = IsAudioRequested,
            IsHostConnected = IsHostConnected,
            SessionType = Type,
            IsHostMustApprove = IsHostMustApprove,
            Peers = GetAllPeersAsModel()
        };

    /// <summary>
    /// Broadcast message to all room members (exclude host)
    /// </summary>
    /// <param name="brMessage"></param>
    [Obsolete] private void BroadCastMessage(BaseMessage brMessage)
    {
        var peers = _videoPeerManager.GetAllPeers();
        foreach (var peer in peers)
        {
            _coordinator.SendMessageToUser(peer.GetUserId(), brMessage);
        }
    }

    public IPeer CreatePeer(string userId)
    {
        return _videoPeerManager.CreateNewPeer(userId, Guid.NewGuid().ToString(), this.AsModel());
    }
    
    public JoinSessionResult AttachPeer(IPeer peer)
    {
        return _videoPeerManager.AddPeer(peer);
    }

    public ApproveUserSessionResult ApprovePeerByUserId(string userId)
    {
        if (!GetPeerByUserId(userId, out var peer))
        {
            return ApproveUserSessionResult.ParticipantNotExists;
        }
        
        return _videoPeerManager.ApprovePeer(peer!.GetId());
    }
    
    public ApproveUserSessionResult ApprovePeerByPeerId(string peerId)
    {
        return _videoPeerManager.ApprovePeer(peerId);
    }

    public RejectUserSessionResult RejectPeerByUserId(string userId)
    {
        if (!GetPeerByUserId(userId, out var peer))
        {
            return RejectUserSessionResult.ParticipantNotExists;
        }
        
        return _videoPeerManager.RejectPeer(peer!.GetId());
    }

    public RejectUserSessionResult RejectPeerByPeerId(string peerId)
    {
        return _videoPeerManager.RejectPeer(peerId);
    }

    public KickFromSessionResult DetachPeer(string peerId, bool isForce)
    {
        return _videoPeerManager.RemovePeer(peerId, isForce);
    }

    public void DetachAllPeers()
    {
        _videoPeerManager.RemoveAllPeers();
    }

    public bool GetPeerById(string? peerId, out IPeer? peer)
    {
        if (_videoPeerManager.GetPeerById(peerId, out var foundPeer))
        {
            peer = foundPeer;
            return true;
        }
        peer = null;
        return false;
    }

    public bool GetPeerByUserId(string userId, out IPeer? peer)
    {
        if (_videoPeerManager.GetPeerByUserId(userId, out var foundPeer))
        {
            peer = foundPeer;
            return true;
        }
        peer = null;
        return false;
    }

    public List<IPeer> GetAllPeers()
    {
        return _videoPeerManager.GetAllPeers();
    }
    
    public List<PeerDTO> GetAllPeersAsModel()
    {
        List<PeerDTO> peerDTOList = new();
        
        foreach (var peer in _videoPeerManager.GetAllPeers())
        {
            peerDTOList.Add(new PeerDTO()
            {
                Id = peer.GetId(),
                UserId = peer.GetUserId(),
                IsAudioRequested = peer.IsAudioRequested,
                IsStreamHost = peer.IsStreamHost,
                PeerState = peer.PeerState,
                IsApproved = peer.IsApproved,
                IsRejected = peer.IsRejected,
                IsConnected = peer.IsConnected,
            });
        }
        
        return peerDTOList;
    }

    public List<UserDTO> GetAllPeersAsUsers()
    {
        return _videoPeerManager.GetAllPeersAsUsers();
    }

    public List<UserDTO> GetAllPeersAsUsersModel()
    {
        throw new NotImplementedException();
    }
    
    public int GetAllPeersCount()
    {
        return _videoPeerManager.GetAllPeersCount();
    }

    public string GetSessionId()
    {
        return _context.Id;
    }

    public string GetName()
    {
        return _context.Name;
    }

    public string? GetHostId()
    {
        return _context.HostId;
    }

    public string? GetHostPeerId()
    {
        return _context.HostPeerId;
    }

    public int GetSessionCapacity()
    {
        return _context.Capacity;
    }

    public void SetHostPeerId(string? hostPeerId)
    {
        _context.HostPeerId = hostPeerId;
    }

    public void ChangeHostState(bool isHostConnected)
    {
        IsHostConnected = isHostConnected;
    }
    
    public void CloseSession()
    {
        DetachAllPeers();
    }
}