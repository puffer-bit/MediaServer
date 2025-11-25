using System;
using System.Threading.Tasks;
using Client.Services.Server.Coordinator;
using Client.Services.Server.Coordinator.UserManager;
using Client.Services.Server.Video.Peer;
using ReactiveUI;
using Shared.Enums;
using Shared.Models.DTO;

namespace Client.ViewModels.Sessions.VideoSession;

public class VideoSessionParticipantViewModel : ReactiveObject
{
    private readonly ICoordinatorSession _coordinatorSession;
    public PeerDTO Peer { get; set; }
    
    private string _userId;
    public string UserId
    {
        get => _userId;
        set => this.RaiseAndSetIfChanged(ref _userId, value);
    }

    private string? _username;
    public string? Username
    {
        get => _username;
        set => this.RaiseAndSetIfChanged(ref _username, value);
    }

    private string? _displayName;
    public string? DisplayName
    {
        get => _displayName;
        set => this.RaiseAndSetIfChanged(ref _displayName, value);
    }
    private bool _isHost;
    public bool IsHost
    {
        get => _isHost;
        private set => this.RaiseAndSetIfChanged(ref _isHost, value);
    }

    private bool _isNegotiated;
    public bool IsNegotiated
    {
        get => _isNegotiated;
        private set => this.RaiseAndSetIfChanged(ref _isNegotiated, value);
    }

    private bool _isConnected;
    public bool IsConnected
    {
        get => _isConnected;
        private set => this.RaiseAndSetIfChanged(ref _isConnected, value);
    }

    private bool _isApproved;
    public bool IsApproved
    {
        get => _isApproved;
        set => this.RaiseAndSetIfChanged(ref _isApproved, value);
    }

    private VideoSessionPeerState _state;
    public VideoSessionPeerState State
    {
        get => _state;
        set => this.RaiseAndSetIfChanged(ref _state, value);
    }
    
    private int _opacity;
    public int Opacity
    {
        get => _opacity;
        set => this.RaiseAndSetIfChanged(ref _opacity, value);
    }
    
    public VideoSessionParticipantViewModel(PeerDTO peerModel, ICoordinatorSession coordinatorSession)
    {
        _coordinatorSession = coordinatorSession;
        
        Peer = peerModel;
        UserId = peerModel.Id;
        IsHost = peerModel.IsStreamHost;
        State = peerModel.PeerState;
        _ = GetRemoteUsername(peerModel.UserId);
    }

    private async Task GetRemoteUsername(string userId)
    {
        Username = (await _coordinatorSession.GetRemoteUser(Peer.UserId))?.Username;
    }
    
    public void ChangeState(VideoSessionPeerState state)
    {
        State = state;
        if (State is not VideoSessionPeerState.WaitingForApprove)
            IsApproved = false;
        else
            IsApproved = true;
    }
}