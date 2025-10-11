using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Shared.Models;

namespace Client.ViewModels;

public interface ISessionViewModel : IDisposable
{
    string Id { get; }
    string Name { get; }
    ObservableCollection<UserDTO> SessionMembers { get; }

    event Action? RequestClose;
    event Action? RequestFullScreen;
    
    Task JoinSession();
    void CloseSession();
}