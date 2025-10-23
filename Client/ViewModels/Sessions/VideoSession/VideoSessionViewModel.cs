using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Client.Services.Other.FrameProcessor;
using Client.Services.Server.Coordinator;
using Client.Services.Server.Video;
using Client.ViewModels.MessageBox;
using Client.Views.MessageBox;
using ReactiveUI;
using Shared.Enums;
using Shared.Models;
using SIPSorceryMedia.Abstractions;

namespace Client.ViewModels.Sessions.VideoSession;

internal class VideoSessionViewModel : ReactiveObject, ISessionViewModel
{
    public string Id
    {
        get => _videoSession?.Id ?? "null";
        set
        {
            if (_videoSession != null)
            {
                _videoSession.Id = value;
                this.RaisePropertyChanged();
            }
        } 
    }
    public string? HostId
    {
        get => _videoSession?.HostId ?? "null";
        set
        {
            if (_videoSession != null)
            {
                _videoSession.HostId = value;
                this.RaisePropertyChanged();
            }
        } 
    }
    public string Name
    {
        get => _videoSession?.Name ?? "null";
        set
        {
            if (_videoSession != null)
            {
                _videoSession.Name = value;
                this.RaisePropertyChanged();
            }
        } 
    }
    public int Capacity
    {
        get => _videoSession?.Capacity ?? 0;
        set
        {
            if (_videoSession != null)
            {
                _videoSession.Capacity = value;
                this.RaisePropertyChanged();
            }
        } 
    }
    public string HostPeerId
    {
        get => _videoSession?.HostPeerId ?? "null";
        set
        {
            if (_videoSession != null)
            {
                _videoSession.HostPeerId = value;
                this.RaisePropertyChanged();
            }
        } 
    }
    public bool IsHostConnected
    {
        get => _videoSession?.IsHostConnected ?? false;
        set
        {
            if (_videoSession != null)
            {
                _videoSession.IsHostConnected = value;
                this.RaisePropertyChanged();
            }
        } 
    }

    private bool _isFullScreen;
    public bool IsFullScreen
    {
        get => _isFullScreen;
        set => this.RaiseAndSetIfChanged(ref _isFullScreen, value);
    }
    public bool IsAudioRequested
    {
        get => _videoSession?.IsAudioRequested ?? false;
        set
        {
            if (_videoSession != null)
            {
                _videoSession.IsAudioRequested = value;
                this.RaisePropertyChanged();
            }
        } 
    }

    private bool IsClosed { get; set; }

    private string _currentAction = "Connecting to session...";
    public string CurrentAction
    {
        get => _currentAction;
        set => this.RaiseAndSetIfChanged(ref _currentAction, value);
    }

    public bool IsLoading => _videoSession == null ||
                            _videoSession.State != VideoSessionState.Connected &&
                            _videoSession.State != VideoSessionState.Failed &&
                            _videoSession.State != VideoSessionState.Ended &&
                            _videoSession.State != VideoSessionState.Canceled;
    
    public readonly VideoSessionDTO SessionDTO;
    
    public ObservableCollection<UserDTO> SessionMembers { get; } = new();
    private IDisposable? _connectionSubscription;
    public Interaction<MessageBoxViewModel, Unit> ShowMessageBox { get; }
    public ReactiveCommand<Unit, Unit> JoinCommand { get; }
    public ReactiveCommand<Unit, Unit> CloseCommand { get; }
    public ReactiveCommand<Unit, Unit> StartReceiveCommand { get; }
    public ReactiveCommand<Unit, Unit> EndReceiveCommand { get; }
    public ReactiveCommand<Unit, Unit> FullScreenCommand { get; }
    public ReactiveCommand<Unit, Unit> StartUserStream { get; }
    
    private WriteableBitmap? _currentFrame;
    public WriteableBitmap? CurrentFrame
    {
        get => _currentFrame;
        set
        {
            this.RaiseAndSetIfChanged(ref _currentFrame, value);
            this.RaisePropertyChanged();
        }
    }

    public event Action? RequestClose;
    public event Action? RequestFullScreen;

    private readonly ICoordinatorSession _coordinatorSession;
    private IVideoSession? _videoSession;
    
    public VideoSessionViewModel(
        VideoSessionDTO sessionDTO, 
        ICoordinatorSession coordinatorSession)
    {
        _coordinatorSession =  coordinatorSession;
        SessionDTO = sessionDTO;
        
        ShowMessageBox = new Interaction<MessageBoxViewModel, Unit>();
        
        JoinCommand = ReactiveCommand.CreateFromTask(JoinSession);
        CloseCommand = ReactiveCommand.Create(CloseSession);
        StartReceiveCommand = ReactiveCommand.Create(StartReceive);
        StartUserStream = ReactiveCommand
            .CreateFromTask(StartSending,
            outputScheduler: RxApp.MainThreadScheduler);
        FullScreenCommand = ReactiveCommand.Create(ToggleFullscreen);
    }

    public async Task JoinSession()
    {
        await Task.Delay(200);
        try
        {
            var result = await _coordinatorSession.JoinSession(SessionDTO);
            if (result == JoinSessionResult.NoError && _coordinatorSession.ConnectionStatus == CoordinatorState.Connected)
            {
                _connectionSubscription?.Dispose();
                _videoSession = _coordinatorSession.GetVideoSessionById(SessionDTO.Id);
                if (_videoSession == null)
                {
                    await ShowMessageBox.Handle(new MessageBoxViewModel(Icon.Error, Buttons.Ok, "Failed to retrieve session data.", "Error", false));
                    RequestClose?.Invoke();
                    return;
                }
                await _videoSession.RefreshSession();
                _connectionSubscription = _videoSession
                    .WhenAnyValue(x => x.State)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Do(_ =>
                    {
                        this.RaisePropertyChanged(nameof(_videoSession.State));
                        this.RaisePropertyChanged(nameof(IconPath));
                        this.RaisePropertyChanged(nameof(IsLoading));
                        HandleSessionStatusAsync(_videoSession.State);
                    })
                    .SelectMany(async status =>
                    {
                        switch (status)
                        {
                            case VideoSessionState.WaitingForUserStream:
                                return StartUserStream;

                            case VideoSessionState.TimedOut:

                            case VideoSessionState.Connected:
                                return StartReceiveCommand;

                            default:
                                return null;
                        }
                    })
                    .Where(cmd => cmd != null)
                    .Subscribe(cmd => cmd?.Execute(Unit.Default));
            }
            else
            {
                if (!IsClosed)
                {
                    var messageBox = new MessageBoxWindow
                    {
                        DataContext = new MessageBoxViewModel(Icon.Error, Buttons.Ok,
                            $"Failed to join session. \n\nError code: {(int)result}",
                            "Error", false)
                    };
                    messageBox.Show();
                }
                RequestClose?.Invoke();
            }
        }
        catch (TimeoutException)
        {
            if (!IsClosed)
            {
                await ShowMessageBox.Handle(new MessageBoxViewModel(Icon.Error, Buttons.Ok, 
                    $"Failed to join session. \n\nTimed out.", 
                    "Error", false));
                _coordinatorSession.LeaveSession(SessionDTO);
                RequestClose?.Invoke();
            }
        }
    }

    public void CloseSession()
    {
        EndSending();
        EndReceive();
        RequestClose?.Invoke();
        _coordinatorSession.LeaveSession(SessionDTO);
        IsClosed = true;
        _videoSession?.Dispose();
        Dispose();
    }
    
    private void StartReceive()
    {
        if (HostId == _coordinatorSession.GetUser().Id)
        {
            return;
        }
        
        if (_videoSession != null && _videoSession.State == VideoSessionState.Connected)
        {
            _videoSession.FrameReceived += OnFrameReceived;
        }
    }
    
    private void EndReceive()
    {
        if (HostId == _coordinatorSession.GetUser().Id)
        {
            return;
        }
        
        if (_videoSession != null && _videoSession.State == VideoSessionState.Connected)
        {
            _videoSession.FrameReceived -= OnFrameReceived;
        }
    }

    private async Task StartSending()
    {
        if (_videoSession != null && _videoSession.State == VideoSessionState.WaitingForUserStream)
        {
            if (await _videoSession.StartSending())
            {
                _videoSession.FrameReceived += OnFrameReceived;
            }
        }
    }
     
    private void EndSending()
    {
        if (_videoSession != null)
        {
            _videoSession.StopSending();
            _videoSession.FrameReceived -= OnFrameReceived;
        }
    }
    
    private void HandleSessionStatusAsync(VideoSessionState status)
    {
        switch (status)
        {
            case VideoSessionState.Connected:
                CurrentAction = $"Connected to \"{Name}\"";
                break;
            case VideoSessionState.Failed:
                CurrentAction = $"Connection to \"{Name}\" failed";
                break;
            case VideoSessionState.WaitingForUserStream:
                CurrentAction = "Waiting for window assigment...";
                break;
            case VideoSessionState.WaitingForNegotiation:
                CurrentAction = "Waiting for RTC connection...";
                break;
            case VideoSessionState.WaitingForHost:
                CurrentAction = "Waiting for host...";
                break;
            case VideoSessionState.TimedOut:
                CurrentAction = "Connection lost. Reconnecting...";
                break;
            case VideoSessionState.Disconnected:
                CurrentAction = "Disconnected. Reconnecting...";
                break;
            case VideoSessionState.Canceled:
                CurrentAction = "Canceled";
                break;
        }
    }
    private void OnFrameReceived(WriteableBitmap bmp)
    {
        CurrentFrame = bmp;
    }

    public void ToggleFullscreen()
    {
        RequestFullScreen?.Invoke();
    }
    
    public string IconPath => _videoSession?.State switch
    {
        VideoSessionState.Connected => "avares://Client/Assets/Icons/Yes.svg",
        VideoSessionState.Failed    => "avares://Client/Assets/Icons/No.svg",
        _ => "avares://Client/Assets/Icons/Info.svg"
    };
    
    public void Dispose()
    {
        _connectionSubscription?.Dispose();
        _videoSession?.Dispose();
        _currentFrame?.Dispose();
    }
}