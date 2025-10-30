using System.Diagnostics.CodeAnalysis;
using Server.MainServer.Main.Server.Coordinator;
using Server.MainServer.Main.Server.Video.Peer;
using Server.MainServer.Main.Server.Video.PeerManager;
using Shared.Enums;
using Shared.Models;
using Shared.Models.DTO;
using Shared.Models.Requests.SessionInfo;

namespace Server.MainServer.Main.Server.Video;

public class VideoSession : IVideoSession
{
    public required Mutex Mutex { get; set; }
    private List<UserDTO> AllowedUsers { get; set; } = []; 
    public bool IsAudioRequested { get; set; }
    public bool IsHostMustApprove { get; set; }
    private bool _isHostConnected;
    public bool IsHostConnected
    {
        get => _isHostConnected;
        set
        {
            if (_isHostConnected != value)
            {
                _isHostConnected = value;
                HostStatusChanged.Invoke();
            }
        }
    }

    public SessionType Type => SessionType.Video;
    public event Action HostStatusChanged;
        
    private readonly ILogger _logger;
    private readonly IVideoSessionContext _context;
    private readonly IVideoPeerManager _videoPeerManager;
    private readonly CoordinatorInstance _coordinator;
        
    [SetsRequiredMembers]
    public VideoSession (
        IVideoSessionContext context, 
        IVideoPeerManager videoPeerManager,
        CoordinatorInstance coordinator,
        bool isAudioRequested,
        bool isHostMustApprove,
        ILoggerFactory loggerFactory)
    {
        _context = context;
        _logger = loggerFactory.CreateLogger($"Session({context.Id})");
        _coordinator = coordinator;
        IsAudioRequested = isAudioRequested;
        IsHostMustApprove = isHostMustApprove;
        _videoPeerManager = videoPeerManager;
        Mutex = new Mutex(false);
        HostStatusChanged += () =>
        {
            if (IsHostConnected)
            {
                BroadCastMessage(new BaseMessage(
                        MessageType.SessionsStateChanged, 
                        new SessionStateChanged(
                            AsModel(), 
                            SessionStateChangedType.HostConnected)),
                    true);
            }
            else
            {
                BroadCastMessage(new BaseMessage(
                        MessageType.SessionsStateChanged, 
                        new SessionStateChanged(
                            AsModel(), 
                            SessionStateChangedType.HostDisconnected)),
                    true);
            }
        };
    }
        
    /// <summary>
    /// Convert Sessions to SessionDTO
    /// </summary>
    /// <returns> RoomModel </returns>
    public VideoSessionDTO AsModel() =>
        new VideoSessionDTO()
        {
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
    /// TODO: Remove host excluding
    /// </summary>
    /// <param name="brMessage"></param>
    /// <param name="excludeHost"></param>
    private void BroadCastMessage(BaseMessage brMessage, bool excludeHost)
    {
        var peers = _videoPeerManager.GetAllPeers();
        foreach (var peer in peers)
        {
            _coordinator.SendMessageToUser(peer.GetUserId(), brMessage);
        }
    }

    public void CreateAndAttachPeer(string userId)
    {
        AttachPeer(_videoPeerManager.CreateNewPeer(userId, Guid.NewGuid().ToString(), this.AsModel()));
    }

    public void AttachPeer(IPeer peer)
    {
        _videoPeerManager.AddPeer(peer);
    }

    public void DetachPeer(string? peerId)
    {
        _videoPeerManager.RemovePeer(peerId);
    }

    public void ChangePeerApproveState(string? peerId, VideoSessionApproveState newState)
    {
        _videoPeerManager.ChangePeerApproveState(peerId, newState);
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
                ApproveState = peer.ApproveState
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

    public string GetSessionId()
    {
        return _context.Id;
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
}