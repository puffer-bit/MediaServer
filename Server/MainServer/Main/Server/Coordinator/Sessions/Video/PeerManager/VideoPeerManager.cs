using Server.MainServer.Main.Server.Coordinator.Sessions.Video.Peer;
using Server.MainServer.Main.Server.Factories.PeerManagerFactory;
using Shared.Enums;
using Shared.Models;
using Shared.Models.DTO;

namespace Server.MainServer.Main.Server.Coordinator.Sessions.Video.PeerManager;

public class VideoPeerManager : IVideoPeerManager
{
    private readonly IVideoPeerManagerContext _context;
    private readonly ILogger _logger;
    private readonly IPeerManagerFactory _peerManagerFactory;
    private readonly CoordinatorInstance _coordinator;
    private readonly IVideoSession _videoSession;

    public VideoPeerManager(IVideoPeerManagerContext context,
        ILoggerFactory loggerFactory,
        IPeerManagerFactory peerManagerFactory,
        IVideoSession videoSession,
        CoordinatorInstance coordinator)
    {
        _context = context;
        _peerManagerFactory = peerManagerFactory;
        _coordinator = coordinator;
        _videoSession = videoSession;
        _logger = loggerFactory.CreateLogger($"Session({_videoSession.GetSessionId()})");
    }
    
    public IPeer CreateNewPeer(string userId, string peerId, VideoSessionDTO videoSessionDTO)
    {
        var peer = _peerManagerFactory.CreatePeer(peerId, userId, videoSessionDTO.IsAudioRequested);
        
        if (peer.GetUserId() == videoSessionDTO.HostId)
        {
            peer.MakeHost();
            peer.PeerState = VideoSessionPeerState.WaitingForNegotiation;
            return peer;
        }

        if (!_videoSession.IsHostMustApprove)
        {
            peer.IsApproved = true;
        }
        
        peer.PeerState = videoSessionDTO.IsHostMustApprove ? VideoSessionPeerState.WaitingForApprove : VideoSessionPeerState.WaitingForNegotiation;
        return peer;
    }

    /// <summary>
    /// Adds peer to session
    /// </summary>
    /// <param name="peer">Instance of the peer being added</param>
    /// <param name="isForce">If true, ignores all session restrictions. Default false.</param>
    /// <param name="isSilent">If true, participants will not hear the sound notification. Default false.</param>
    public JoinSessionResult AddPeer(IPeer peer, bool isForce = false, bool isSilent = false)
    {
        if (GetPeerByUserId(peer.GetUserId(), out var alreadyExistedPeer))
        {
            RemovePeer(alreadyExistedPeer!.GetId(), false, true);
            _logger.LogTrace("Peer with ID {peerId} already existed.", alreadyExistedPeer.GetUserId());
        }
        
        peer.IsConnected = true;
        if (peer.IsStreamHost) // Host connected
        {
            if (_context.Peers.TryAdd(peer.GetId(), peer))
            {
                _coordinator.CreateNewWebRTCPeer(peer.GetUserId(), _videoSession.GetSessionId(), peer.GetId(), true,
                    peer.IsAudioRequested);
                _coordinator.SetVideoSessionHost(_videoSession!.AsModel(), peer.GetId());
                
                _logger.LogTrace("Host with ID {peerId} successful connected.", peer.GetUserId());
                if (!isSilent)
                    _coordinator.HostJoinedVideoSession(peer.GetUserId(), _videoSession.GetSessionId());
                return JoinSessionResult.NoError;
            }
            
            _logger.LogDebug("Host with ID {peerId} failed to connect. Internal error.", peer.GetUserId());
        }
        else // User connected
        {
            if ((_videoSession.GetSessionCapacity() < _videoSession.GetAllPeersCount() + 1) & !isForce)
            {
                _logger.LogTrace("Client with ID {peerId} failed to connect. Room full.", peer.GetUserId());
                return JoinSessionResult.RoomFull;
            }
                
            if (_context.Peers.TryAdd(peer.GetId(), peer))
            {
                _coordinator.CreateNewWebRTCPeer(peer.GetUserId(), _videoSession.GetSessionId(), peer.GetId(), false, peer.IsAudioRequested);
                
                if (isForce)
                    _logger.LogTrace("Client with ID {peerId} forcibly successful connected.", peer.GetUserId());
                else
                    _logger.LogTrace("Client with ID {peerId} successful connected.", peer.GetUserId());
                if (!isSilent)
                    _coordinator.UserJoinedVideoSession(peer.GetUserId(), _videoSession.GetSessionId());
                return JoinSessionResult.NoError;
            }
            
            _logger.LogDebug("Client with ID {peerId} failed to connect. Internal error.", peer.GetUserId());
        }

        return JoinSessionResult.InternalError;
    }

    public ApproveUserSessionResult ApprovePeer(string peerId)
    {
        if (peerId == _videoSession.GetHostPeerId())
        {
            _logger.LogDebug("Attempt to approve a peer with ID {peerId} failed. Host cannot be approved.", peerId);
            return ApproveUserSessionResult.InternalError;
        }
        
        if (_context.Peers.TryGetValue(peerId, out var peer))
        {
            if (peer.IsRejected)
            {
                _logger.LogTrace("Attempt to reject a peer with ID {peerId} failed. Participant already rejected.", peer.GetUserId());
                return ApproveUserSessionResult.AlreadyRejected;
            }
            
            ChangePeerState(peer, VideoSessionPeerState.Approved);
            _logger.LogTrace("Client with ID {peerId} approved.", peer.GetUserId());
            _coordinator.UserApprovedInVideoSession(peer.GetUserId(), _videoSession.GetSessionId());
            return ApproveUserSessionResult.NoError;
        }
        
        _logger.LogDebug("Attempt to approve a peer with ID {peerId} failed. Peer not exists.", peerId);
        return ApproveUserSessionResult.ParticipantNotExists;
    }

    public RejectUserSessionResult RejectPeer(string peerId)
    {
        if (peerId == _videoSession.GetHostPeerId())
        {
            _logger.LogDebug("Attempt to reject a peer with ID {peerId} failed. Host cannot be rejected.", peerId);
            return RejectUserSessionResult.InternalError;
        }
        
        if (_context.Peers.TryGetValue(peerId, out var peer))
        {
            if (peer.IsApproved)
            {
                _logger.LogTrace("Attempt to reject a peer with ID {peerId} failed. Participant already approved.", peer.GetUserId());
                return RejectUserSessionResult.AlreadyApproved;
            }
            ChangePeerState(peer, VideoSessionPeerState.Rejected);
            _logger.LogTrace("Client with ID {peerId} rejected.", peer.GetUserId());
            _coordinator.UserRejectedInVideoSession(peer.GetUserId(), _videoSession.GetSessionId());
            return RejectUserSessionResult.NoError;
        }
        
        _logger.LogDebug("Attempt to reject a peer with ID {peerId} failed. Peer not exists.", peerId);
        return RejectUserSessionResult.ParticipantNotExists;
    }

    public KickFromSessionResult RemovePeer(string peerId, bool isForce = false, bool isSilent = false)
    {
        if (_context.Peers.TryRemove(peerId, out var peer))
        {
            peer.IsConnected = false;
            _coordinator.DisposeWebRTCPeer(peer.GetId());
            if (peer.IsStreamHost)
            {
                _coordinator.RemoveVideoSessionHost(_videoSession.AsModel());
                if (isForce)
                {
                    _logger.LogTrace("Host with ID {peerId} kicked.", peer.GetUserId());
                    if (!isSilent)
                        _coordinator.HostKickedFromVideoSession(peer.GetId(), _videoSession.GetSessionId());
                    return KickFromSessionResult.NoError;
                }
                
                _logger.LogTrace("Host with ID {peerId} leaved.", peer.GetUserId());
                if (!isSilent)
                    _coordinator.HostLeavesVideoSession(peer.GetId(), _videoSession.GetSessionId());
                return KickFromSessionResult.NoError;
            }

            if (isForce)
            {
                _logger.LogTrace("Client with ID {peerId} kicked.", peer.GetUserId());
                if (!isSilent)
                    _coordinator.UserKickedFromVideoSession(peer.GetId(), _videoSession.GetSessionId());
                return KickFromSessionResult.NoError;
            }
            
            _logger.LogTrace("Client with ID {peerId} leaved.", peer.GetUserId());
            if (!isSilent)
                _coordinator.UserLeavesVideoSession(peer.GetId(), _videoSession.GetSessionId());
            return KickFromSessionResult.NoError;
        }
        
        _logger.LogDebug("Attempt to remove a peer with ID {peerId} failed. Peer not exists.", peerId);
        return KickFromSessionResult.ParticipantNotExists;
    }

    private void ChangePeerState(IPeer peer, VideoSessionPeerState newState)
    {
        if (newState == VideoSessionPeerState.Approved)
        {
            peer.IsApproved = true;
        }
        else if (newState == VideoSessionPeerState.Rejected)
        {
            peer.IsRejected = true;
        }
        peer.PeerState = newState;
    }
    
    public IVideoPeerManagerContext GetPeerManagerContext()
    {
        return _context;
    }

    public List<IPeer> GetAllPeers()
    {
        return _context.Peers.Values.ToList();
    }
    
    public int GetAllPeersCount()
    {
        return _context.Peers.Count;
    }

    public List<UserDTO> GetAllPeersAsUsers()
    {
        var users = new List<UserDTO>();
        foreach (var peer in _context.Peers.Values)
        {
            if (_coordinator.GetUser(peer.GetUserId(), out var user))
            {
                users.Add(user!);
            }
        }
        return users;
    }

    public bool GetPeerById(string? peerId, out IPeer? videoPeer)
    {
        if (_context.Peers.TryGetValue(peerId, out var peer))
        {
            videoPeer = peer;
            return true;
        }
        videoPeer = null;
        return false;
    }

    public bool GetPeerByUserId(string userId, out IPeer? videoPeer)
    {
        foreach (var peer in _context.Peers)
        {
            if (peer.Value.GetUserId() == userId)
            {
                videoPeer = peer.Value;
                return true;
            }
        }
        videoPeer = null;
        return false;
    }

    public void RemoveAllPeers()
    {
        _context.Peers.Clear();
    }
}