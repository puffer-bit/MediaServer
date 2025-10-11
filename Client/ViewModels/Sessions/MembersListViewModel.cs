using System.Collections.ObjectModel;
using Client.Services.Server.Coordinator;
using ReactiveUI;
using Shared.Models;

namespace Client.ViewModels.Sessions;

public class MembersListViewModel : ReactiveObject
{
    private readonly ICoordinatorSession _coordinatorSession;
    private ObservableCollection<UserDTO> _sessionMembers = new();
    
    public MembersListViewModel(ICoordinatorSession coordinatorSession)
    {
        _coordinatorSession = coordinatorSession;
        
    }
}