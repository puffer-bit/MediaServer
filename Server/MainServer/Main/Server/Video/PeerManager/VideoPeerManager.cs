using Server.MainServer.Main.Server.Coordinator;
using Server.MainServer.Main.Server.Factories.PeerManagerFactory;
using Server.MainServer.Main.Server.Video.Peer;
using Shared.Models;

namespace Server.MainServer.Main.Server.Video.PeerManager;

public class VideoPeerManager : IVideoPeerManager
{
    private readonly IVideoPeerManagerContext _context;
    private readonly ILogger _logger;
    private readonly IPeerManagerFactory _peerManagerFactory;
    private readonly CoordinatorInstance _coordinator;
    public string RoomId { get; set; }

    public VideoPeerManager(IVideoPeerManagerContext context,
        ILoggerFactory loggerFactory,
        IPeerManagerFactory peerManagerFactory,
        CoordinatorInstance coordinator, 
        string roomId)
    {
        _context = context;
        _peerManagerFactory = peerManagerFactory;
        _coordinator = coordinator;
        RoomId = roomId;
        _logger = loggerFactory.CreateLogger($"Session({RoomId})");
    }
    
    public IPeer CreateNewPeer(string userId, string peerId, SessionDTO sessionDTO)
    {
        VideoSessionDTO videoSessionDTO = (VideoSessionDTO)sessionDTO;
        var peer = _peerManagerFactory.CreatePeer(peerId, userId, videoSessionDTO.IsAudioRequested);
        if (peer.GetUserId() == videoSessionDTO.HostId)
        {
            peer.MakeHost();
        }

        return peer;
    }

    /// <summary>
    /// Adds peer to session
    /// </summary>
    /// <param name="peer"></param>
    public void AddPeer(IPeer peer)
    {
        if (GetPeerByUserId(peer.GetUserId(), out var videoPeer))
        {
            _context.Peers.TryRemove(videoPeer!.GetId(), out _);
            _logger.LogInformation("Peer with ID {peerId} already existed in Room {RoomId}.", videoPeer.GetUserId(), RoomId);
        }
        
        // User connected
        if (peer.IsStreamHost)
        {
            if (_context.Peers.TryAdd(peer.GetId(), peer))
            {
                _logger.LogInformation("Host with ID {peerId} connected to Room {roomId}.", peer.GetUserId(), RoomId);
                _coordinator.CreateNewWebRTCPeer(peer.GetUserId(), RoomId, peer.GetId(), true,
                    peer.IsAudioRequested);
                _coordinator.GetVideoSession(RoomId, out var videoSession);
                _coordinator.SetVideoSessionHost(videoSession.AsModel(), peer.GetId());
            }
            else
            {
                _logger.LogInformation("Host with ID {peerId} already existed, replaced.", peer.GetUserId());
                _context.Peers[peer.GetId()] =  peer;
                _coordinator.CreateNewWebRTCPeer(peer.GetUserId(), RoomId, peer.GetId(), true, peer.IsAudioRequested);
                _coordinator.GetVideoSession(RoomId, out var videoSession);
                _coordinator.SetVideoSessionHost(videoSession.AsModel(), peer.GetId());
            }
        }
        else
        {
            if (_context.Peers.TryAdd(peer.GetId(), peer))
            {
                _logger.LogInformation("Client with ID {peerId} connected to Room {roomId}.", peer.GetUserId(), RoomId);
                _context.Peers[peer.GetId()] =  peer;
                _coordinator.CreateNewWebRTCPeer(peer.GetUserId(), RoomId, peer.GetId(), false, peer.IsAudioRequested);
            }
            else
            {
                _logger.LogInformation("Client with ID {peerId} already existed, replaced.", peer.GetUserId());
                _context.Peers[peer.GetId()] =  peer;
                _coordinator.CreateNewWebRTCPeer(peer.GetUserId(), RoomId, peer.GetId(), false, peer.IsAudioRequested);
            }
        }
    }
    
    public bool RemovePeer(string? peerId)
    {
        if (_context.Peers.TryGetValue(peerId, out var peer))
        {
            if (peer.IsStreamHost)
            {
                _coordinator.GetVideoSession(RoomId, out var videoSession);
                _coordinator.DisposeWebRTCPeer(peerId);
                _coordinator.RemoveVideoSessionHost(videoSession.AsModel());
                _logger.LogInformation("Host with ID {peerId} disconnected from Room {RoomId}.", _context.Peers[peerId].GetUserId(), RoomId);
            }
            else
            {
                _logger.LogInformation("Client with ID {peerId} disconnected from Room {RoomId}.", _context.Peers[peerId].GetUserId(), RoomId);
            }
        }
        return _context.Peers.TryRemove(peerId, out _);
    }

    public IVideoPeerManagerContext GetPeerManagerContext()
    {
        return _context;
    }

    public List<IPeer> GetAllPeers()
    {
        return _context.Peers.Values.ToList();
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
}