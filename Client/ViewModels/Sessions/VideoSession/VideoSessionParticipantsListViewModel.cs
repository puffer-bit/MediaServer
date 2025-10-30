using System.Collections.ObjectModel;
using System.Reactive;
using Client.Services.Server.Coordinator;
using Client.Services.Server.Video;
using ReactiveUI;
using Shared.Enums;
using Shared.Models;

namespace Client.ViewModels.Sessions.VideoSession;

public class VideoSessionParticipantsListViewModel : ReactiveObject
{
    private readonly ICoordinatorSession _coordinatorSession;
    
    // UI Collections
    public ObservableCollection<VideoSessionParticipantViewModel> Participants { get; set; } = new();
    
    // UI Commands
    public ReactiveCommand<Unit, Unit> ApproveParticipant { get; init; }
    public ReactiveCommand<Unit, Unit> RejectParticipant { get; init; }
    public ReactiveCommand<Unit, Unit> KickParticipant { get; init;}
    public ReactiveCommand<Unit, Unit> BanParticipant { get; init; }
    
    private VideoSessionParticipantViewModel? _selectedParticipant;
    public VideoSessionParticipantViewModel? SelectedParticipant
    {
        get => _selectedParticipant;
        set => this.RaiseAndSetIfChanged(ref _selectedParticipant, value);
    }
    
    public VideoSessionParticipantsListViewModel(ICoordinatorSession coordinatorSession)
    {
        _coordinatorSession = coordinatorSession;
    }

    public void FetchUsersFromDTO(VideoSessionDTO sessionDto)
    {
        Participants.Clear();
        foreach (var peer in sessionDto.Peers)
        {
            Participants.Add(new VideoSessionParticipantViewModel(peer, _coordinatorSession));
        }
    }
}