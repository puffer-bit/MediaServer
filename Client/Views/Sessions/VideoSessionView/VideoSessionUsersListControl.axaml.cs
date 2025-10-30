
using Avalonia.ReactiveUI;
using Client.ViewModels.Sessions.VideoSession;

namespace Client.Views.Sessions.VideoSessionView;

public partial class VideoSessionUsersListControl : ReactiveUserControl<VideoSessionParticipantsListViewModel>
{
    public VideoSessionUsersListControl()
    {
        InitializeComponent();
    }
}