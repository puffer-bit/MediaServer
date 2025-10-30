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
    
    public string UserId { get; set; }

    public string? Username { get; set; }
    public string? DisplayName { get; set; }
    public PeerDTO Peer { get; set; }
    public bool IsHost { get; private set; }
    public bool IsNegotiated { get; private set; }
    public bool IsConnected { get; private set; }
    public bool IsApproved { get; set; }
    public VideoSessionApproveState State { get; set; }
    
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
        _ = GetRemoteUsername(peerModel.UserId);
    }

    private async Task GetRemoteUsername(string userId)
    {
        Username = (await _coordinatorSession.GetRemoteUser(Peer.UserId))?.Username;
    }
    
    public void ChangeState(VideoSessionApproveState state)
    {
        State = state;
        if (State is not VideoSessionApproveState.WaitingForApprove)
            IsApproved = false;
        else
            IsApproved = true;
    }
}