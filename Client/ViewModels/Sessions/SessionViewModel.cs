using ReactiveUI;
using Shared.Models;

namespace Client.ViewModels.Sessions;

public class SessionViewModel : ReactiveObject
{
    // UI
    private bool _isEntered;
    public bool IsEntered
    {
        get => _isEntered;
        set => this.RaiseAndSetIfChanged(ref _isEntered, value);
    }
    public bool IsHost { get; set; }

    private int _opacity;
    public int Opacity
    {
        get => _opacity;
        set => this.RaiseAndSetIfChanged(ref _opacity, value);
    }
    
    // Session data
    public SessionDTO Dto { get; }
    public SessionViewModel(SessionDTO dto) => Dto = dto;
}