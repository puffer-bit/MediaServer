using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Client.Services.Other;
using Client.Services.Other.AppInfrastructure;
using Client.Services.Server.Coordinator;
using Client.ViewModels.MessageBox;
using Client.ViewModels.Sessions;
using Client.ViewModels.Sessions.VideoSession;
using ReactiveUI;
using Shared.Enums;
using Shared.Models;
using Shared.Models.DTO;

namespace Client.ViewModels.MainWindow;

internal class MainWindowViewModel : ReactiveObject
{
    // Coordinator session
    private ICoordinatorSession? _coordinatorSession;
    private CancellationTokenSource _connectionCancellationSource = new();
    
    // UI parameters
    private string _currentAction = "Offline";
    public string CurrentAction
    {
        get => _currentAction;
        set
        {
            this.RaiseAndSetIfChanged(ref _currentAction, value);
            this.RaisePropertyChanged(nameof(IsLoading));
        }
    }
    
    private ISessionViewModel? _currentContent;
    public ISessionViewModel? CurrentContent
    {
        get => _currentContent;
        set => this.RaiseAndSetIfChanged(ref _currentContent, value);
    }
    
    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }
    
    private bool _isConnected;
    public bool IsConnected
    {
        get => _isConnected;
        set => this.RaiseAndSetIfChanged(ref _isConnected, value);
    }

    private bool _isInitialized;
    public bool IsInitialized
    {
        get => _isInitialized;
        set
        {
            this.RaiseAndSetIfChanged(ref _isInitialized, value);
            this.RaisePropertyChanged(nameof(MenuVisibility));
        }
    }

    private WindowState _isFullScreen;
    public WindowState IsFullScreen
    {
        get => _isFullScreen;
        set
        {
            this.RaiseAndSetIfChanged(ref _isFullScreen, value);
            this.RaisePropertyChanged(nameof(MenuVisibility));
        }
    }

    private bool _gridOpacity;
    public bool GridOpacity
    {
        get => _gridOpacity;
        set => this.RaiseAndSetIfChanged(ref _gridOpacity, value);
    }
    
    private bool _currentActionTextOpacity;
    public bool CurrentActionTextOpacity
    {
        get => _currentActionTextOpacity;
        set => this.RaiseAndSetIfChanged(ref _currentActionTextOpacity, value);
    }

    private bool _currentContentOpacity;
    public bool CurrentContentOpacity
    {
        get => _currentContentOpacity;
        set => this.RaiseAndSetIfChanged(ref _currentContentOpacity, value);
    }
    
    public bool MenuVisibility => IsInitialized && IsFullScreen != WindowState.FullScreen;
    public GridLength LeftColumnWidth => IsFullScreen == WindowState.FullScreen ? new GridLength(0) : new GridLength(200);
    
    private ContentType _currentContentType;
    public ContentType CurrentContentType
    {
        get => _currentContentType;
        set => this.RaiseAndSetIfChanged(ref _currentContentType, value);
    }
    
    private double _menuOpacity = 0.6;
    public double MenuOpacity
    {
        get => _menuOpacity;
        set => this.RaiseAndSetIfChanged(ref _menuOpacity, value);
    }

    private UserDTO? _user;
    public UserDTO? User
    {
        get => _user;
        set => this.RaiseAndSetIfChanged(ref _user, value);
    }
    
    // UI Collections
    public ObservableCollection<SessionViewModel> Sessions { get; set; } = new();
    
    // UI Interactions
    public Interaction<Unit, Unit> ShowSettings { get; }
    public Interaction<Unit, Unit> ToggleFullscreenMode { get; }
    public Interaction<Unit, ICoordinatorSession?> ShowConnectWindow { get; }
    public Interaction<MessageBoxViewModel, Result> ShowTestBox { get; }
    public Interaction<CreateSessionWindowViewModel, Unit> ShowCreateSessionWindow { get; }

    // Reactive Commands
    public ReactiveCommand<Unit, Unit> OpenSettingsCommand { get; }
    public ReactiveCommand<Unit, Unit> ConnectCommand { get; }
    public ReactiveCommand<Unit, Unit> DisconnectCommand { get; }
    public ReactiveCommand<SessionViewModel ,Unit> ShowVideoCommand { get; }
    public ReactiveCommand<Unit, Unit> TestCommand { get; }
    public ReactiveCommand<Unit, Unit> CreateSessionCommand { get; }
    public ReactiveCommand<SessionViewModel, Unit> DeleteSessionCommand { get; set; }

    // Subscriptions
    private IDisposable? _connectionSubscription;
    
    // DI
    private readonly AppSettingsManager _appSettingsManager;
    private readonly AppInitializer _appInitializer;
    
    public MainWindowViewModel(
        AppSettingsManager appSettingsManager, 
        ICoordinatorSession coordinatorSession, 
        AppInitializer appInitializer)
    {
        // DI init
        _appSettingsManager = appSettingsManager;
        _appInitializer = appInitializer;
        ToggleFullscreenMode = new Interaction<Unit, Unit>();

        // Reactive Command init
        ShowConnectWindow = new Interaction<Unit, ICoordinatorSession?>();
        ShowTestBox = new Interaction<MessageBoxViewModel, Result>();
        ShowCreateSessionWindow = new Interaction<CreateSessionWindowViewModel, Unit>();
        ShowSettings = new Interaction<Unit, Unit>();

        OpenSettingsCommand = ReactiveCommand
            .CreateFromTask(OpenSettings,
                outputScheduler: RxApp.MainThreadScheduler
            );
            
        ConnectCommand = ReactiveCommand
            .CreateFromTask(ConnectToCoordinator,
                outputScheduler: RxApp.MainThreadScheduler
            );

        DisconnectCommand = ReactiveCommand
            .CreateFromTask(async () =>
                {
                    await DisconnectFromCoordinator();
                    await ChangeCurrentActionText("Offline");
                    _coordinatorSession?.Dispose();
                    _coordinatorSession = null;
                },
                outputScheduler: RxApp.MainThreadScheduler
            );

        CreateSessionCommand = ReactiveCommand
            .CreateFromTask(CreateNewSession, 
                outputScheduler: RxApp.MainThreadScheduler
            );
        
        DeleteSessionCommand = ReactiveCommand
            .CreateFromTask<SessionViewModel>(DeleteSession, 
                outputScheduler: RxApp.MainThreadScheduler
            );
        
        TestCommand = ReactiveCommand
            .CreateFromTask(async () =>
                {
                    var result = await ShowTestBox.Handle(new MessageBoxViewModel(Icon.Info, Buttons.OkCancel, "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s.\n \nWhen an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.", "Lorem Ipsum", false));

                    if (result == Result.Ok)
                    {
                        CurrentAction = "Done";
                    }
                    else
                    {
                        CurrentAction = "Error";
                    }
                },
                outputScheduler: RxApp.MainThreadScheduler
            );
        
        ShowVideoCommand = ReactiveCommand.Create<SessionViewModel>(sessionViewModel =>
        {
            CurrentContent?.CloseSession();
            ChangeContent(new VideoSessionViewModel((VideoSessionDTO)sessionViewModel.Dto, _coordinatorSession!));
            CurrentContent!.JoinSession();
            CurrentContent.RequestClose += () =>
            {
                ChangeContent(null);
                IsFullScreen = WindowState.Normal;
                sessionViewModel.IsEntered = false;
            };
            CurrentContent.RequestFullScreen += ToggleFullscreen;
            sessionViewModel.IsEntered = true;
            ChangeContent(CurrentContent, ContentType.VideoSession);
        });
        
        // Lib and Settings init
        _ = Initialize(coordinatorSession);
    }

    private async Task OpenSettings()
    {
        await ShowSettings.Handle(Unit.Default);
    }

    private async Task Initialize(ICoordinatorSession coordinatorSession)
    {
        await ChangeCurrentActionText("Initialization...");
        await Task.Delay(300);
        
        if (!await _appInitializer.InitializeAsync())
        {
            _appInitializer.GetFailedDependencies(out var failedDependencies);
            if (failedDependencies == null)
            {
                await ShowTestBox.Handle(new MessageBoxViewModel(Icon.Fatal, Buttons.Ok, $"Startup error. " +
                    $"\n\nFailed to obtain required dependencies. Occurred internal error. ", "Fatal error", false));
            }
            else
            {
                await ShowTestBox.Handle(new MessageBoxViewModel(Icon.Fatal, Buttons.Ok, $"Startup error. " +
                    $"\n\nFailed to obtain required dependencies. Check the integrity of the program. " +
                    $"Perhaps the required libraries are damaged or completely missing. Further launch is impossible. \n\n" +
                    $"Failed dependencies: {failedDependencies}", "Fatal error", false));
            }
            Environment.Exit(-1);
            return;
        }

        IsInitialized = true;
        ShowGrid();
        ActivateMenu();
        
        if (_appSettingsManager.SettingsData is { IsAutoConnectEnabled: true, LastIdentity: not null } &&
            _appSettingsManager.SettingsData.CoordinatorSessionsIdentities.TryGetValue(_appSettingsManager.SettingsData.LastIdentity, out var coordinatorDTO))
        {
            await ChangeCurrentActionText($"Connecting to {coordinatorDTO.IpAddress} as {coordinatorDTO.User.Username}...");
            await coordinatorSession.ConnectAndAuthenticate(coordinatorDTO.User, coordinatorDTO.IpAddress);

            if (coordinatorSession.ConnectionStatus != CoordinatorState.Connected)
            {
                await ChangeCurrentActionText("Connection failed");
                await ShowTestBox.Handle(new MessageBoxViewModel(Icon.Error, Buttons.Ok, 
                    "Autoconnection error. \n\nFailed to establish connection to previous server. " +
                    "Perhaps the server address has changed or your login data is no longer valid.", 
                    "Connection error", false));
                coordinatorSession.Dispose();
            }
            else
            {
                _coordinatorSession = coordinatorSession;
                FinalizeCoordinatorConnection();
            }
        }
        else
        {
            await ChangeCurrentActionText("Offline");
        }
    }
        
    private async Task ConnectToCoordinator()
    {
        if (_coordinatorSession != null &&
            _coordinatorSession.ConnectionStatus != CoordinatorState.Connected)
        {
            await _connectionCancellationSource.CancelAsync();
            _connectionCancellationSource = new();
        }
        ICoordinatorSession? newCoordinator = await ShowConnectWindow.Handle(Unit.Default);
        if (newCoordinator != null)
            _coordinatorSession = newCoordinator;
        else
            return;
        
        if (_coordinatorSession.ConnectionStatus == CoordinatorState.Connected)
        {
            FinalizeCoordinatorConnection();
        }
    }
    
    private async Task ReconnectToCoordinator(CancellationToken cancellationToken)
    {
        const int maxReconnectAttempts = 10;
        const int delayBetweenAttemptsMs = 5000;
        int attempt = 0;
        IsConnected = false;
        _connectionSubscription?.Dispose();

        try
        {
            while (attempt < maxReconnectAttempts)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await DisconnectFromCoordinator();
                
                await _coordinatorSession!.Reconnect();
                cancellationToken.ThrowIfCancellationRequested();
                
                if (_coordinatorSession.ConnectionStatus == CoordinatorState.Connected)
                {
                    FinalizeCoordinatorConnection();
                    return;
                }
                await ChangeCurrentActionText($"Connection lost. Reconnecting... (Attempt {attempt + 1})");
                attempt++;
                await Task.Delay(delayBetweenAttemptsMs);
            }
        
            cancellationToken.ThrowIfCancellationRequested();
            await ShowTestBox.Handle(new MessageBoxViewModel(Icon.Fatal, Buttons.Ok, "Connection closed. \n\nFailed to connect after 10 attempts.","Connection", false));
            await ChangeCurrentActionText("Offline");
        }
        catch (OperationCanceledException)
        {
            IsLoading = false;
            await ChangeCurrentActionText("Reconnection cancelled.");
        }
    }

    private void FinalizeCoordinatorConnection()
    {
        _connectionSubscription?.Dispose();
        Sessions.Clear();
        _connectionSubscription = _coordinatorSession
            .WhenAnyValue(x => x.ConnectionStatus)
            .ObserveOn(RxApp.MainThreadScheduler)
            .SelectMany(async status =>
            {
                await HandleConnectionStatusAsync(status);

                switch (status)
                {
                    case CoordinatorState.Connected:
                        await UpdateDataFromCoordinator();
                        break;

                    case CoordinatorState.TimedOut:
                    case CoordinatorState.HeartbeatTimedOut:
                        await ReconnectToCoordinator(_connectionCancellationSource.Token);
                        break;
                    case CoordinatorState.HeartbeatServerShutdown:
                        await RemoteDisconnectFromCoordinator();
                        break;
                }

                return Unit.Default;
            })
            .Subscribe();
        User = _coordinatorSession!.GetUser();
        _currentAction = "Connected";
    }
    
    private async Task RemoteDisconnectFromCoordinator()
    {
        ChangeContent(null);
        DetachSessionManager();
        _connectionSubscription?.Dispose();
        await _coordinatorSession!.Disconnect();
        IsConnected = false;
        User = null;
        _currentAction = "Offline";
        await ShowTestBox.Handle(new MessageBoxViewModel(Icon.Error,
            Buttons.Ok, "Connection closed. " +
                        "\n\nServer has shut down.","Connection", 
            false));

    }
    
    private async Task DisconnectFromCoordinator()
    {
        ChangeContent(null);
        DetachSessionManager();
        _connectionSubscription?.Dispose();
        await _coordinatorSession!.Disconnect();
        IsConnected = false;
        User = null;
    }
    
    private async Task UpdateDataFromCoordinator()
    {
        await ChangeCurrentActionText("Retrieving sessions info...");

        if (_coordinatorSession?.ConnectionStatus == CoordinatorState.Connected)
        {
            await Task.Delay(700);
            await _coordinatorSession.UpdateAllSessionsAsync();
            await AttachSessionManager();
            await ChangeCurrentActionText("Connected");
        }
    }
    
    public async Task AttachSessionManager()
    {
        Sessions.Clear();
        foreach (var sessionDTO in _coordinatorSession.GetAllSessions())
        {
            SessionViewModel sessionViewModel = new SessionViewModel(sessionDTO)
            {
                IsHost = _coordinatorSession.GetUser().Id == sessionDTO.HostId,
            };
            Sessions.Add(sessionViewModel);
            await ActivateListBoxItem(sessionViewModel);
        }

        _coordinatorSession.SessionsCollectionChanged += SessionCollectionChanged;
    }

    public void DetachSessionManager()
    {
        Sessions.Clear();
        _coordinatorSession.SessionsCollectionChanged -= SessionCollectionChanged;
    }
    
    private void SessionCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems != null)
                {
                    foreach (SessionDTO newDto in e.NewItems)
                    {
                        var newSession = new SessionViewModel(newDto)
                        {
                            IsHost = _coordinatorSession.GetUser().Id == newDto.HostId,
                        };
                        Sessions.Add(newSession);
                        _ = ActivateListBoxItem(newSession);
                    }
                }

                break;

            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems != null)
                {
                    foreach (SessionDTO oldDto in e.OldItems)
                    {
                        for (int i = 0; i < Sessions.Count; i++)
                        {
                            if (oldDto.Id == Sessions[i].Dto.Id)
                            {
                                Sessions.Remove(Sessions[i]);
                                if (CurrentContent != null && CurrentContent.Id == oldDto.Id)
                                {
                                    CurrentContent.CloseSession();
                                }
                            }
                        }
                    }
                }

                break;

            case NotifyCollectionChangedAction.Replace:
                if (e.OldItems != null && e.NewItems != null)
                {
                    for (int i = 0; i < e.OldItems.Count; i++)
                    {
                        var oldDto = (SessionDTO)e.OldItems[i]!;
                        var newDto = (SessionDTO)e.NewItems[i]!;

                        var oldVm = Sessions.FirstOrDefault(x => x.Dto.Id == oldDto.Id);
                        if (oldVm != null)
                        {
                            int index = Sessions.IndexOf(oldVm);
                            if (index >= 0)
                            {
                                var newVm = new SessionViewModel(newDto)     
                                {
                                    IsHost = _coordinatorSession.GetUser().Id == newDto.HostId,
                                };
                                Sessions[index] = newVm;
                                _ = ActivateListBoxItem(Sessions[index]);
                            }
                        }
                        else
                        {
                            var newVm = new SessionViewModel(newDto)
                            {
                                IsHost = _coordinatorSession.GetUser().Id == newDto.HostId,
                            };
                            Sessions.Add(newVm);
                        }
                    }
                }
                break;


            case NotifyCollectionChangedAction.Reset:
                Sessions.Clear();
                break;
        }
    }
    
    private async Task CreateNewSession()
    {
        var vm = new CreateSessionWindowViewModel(_coordinatorSession!, SessionType.Video, false);
        await ShowCreateSessionWindow.Handle(vm);
        vm.Dispose();
    }

    private async Task DeleteSession(SessionViewModel sessionViewModel)
    {
        if (await ShowTestBox.Handle(new MessageBoxViewModel(Icon.Question, Buttons.YesNo, 
                "Are you sure you want to delete this session?",
                "Question", false)) == Result.Yes)
        {
            var result = await _coordinatorSession!.DeleteSession(sessionViewModel.Dto);
            if (result != DeleteSessionResult.NoError)
            {
                await ShowTestBox.Handle(new MessageBoxViewModel(Icon.Error, Buttons.Ok,
                    $"Failed to delete session {sessionViewModel.Dto.Name}. \n\nError code: {(int)result}",
                    "Error", false));
            }
        }
    }
    
    private void ShowGrid()
    {
        GridOpacity = true;
    }
    
    private void HideGrid()
    {
        GridOpacity = false;
    }

    private void ActivateMenu()
    {
        MenuOpacity = 1;
    }

    private void DisableMenu()
    {
        MenuOpacity = 0.6;
    }
    
    private async Task ActivateListBoxItem(SessionViewModel sessionViewModel)
    {
        await Task.Delay(100);
        sessionViewModel.Opacity = 1;
    }

    private async Task ChangeCurrentActionText(string newText)
    {
        CurrentActionTextOpacity = false;
        await Task.Delay(150);
        CurrentAction = newText;
        if (newText is "Connected" or "Failed" or "Offline" or "Connection failed")
            IsLoading = false;
        else
            IsLoading = true;
        CurrentActionTextOpacity = true;
    }

    private void ChangeContent(ISessionViewModel? content, ContentType type = ContentType.Undefined)
    {
        if (content == null)
        {
            CurrentContentOpacity = false;
            CurrentContent = null;
            CurrentContentType = ContentType.Undefined;
        }
        else
        {
            CurrentContent = content;
            CurrentContentType = type;
            CurrentContentOpacity = true;
        }
    }
    
    private async Task HandleConnectionStatusAsync(CoordinatorState status)
    {
        switch (status)
        {
            case CoordinatorState.Connected:
                IsConnected = true;
                break;
            case CoordinatorState.Connecting:
                await ChangeCurrentActionText("Waiting for server reply...");
                break;
            case CoordinatorState.Failed:
                await ChangeCurrentActionText("Connection failed");
                break;
            case CoordinatorState.AuthenticationFailed:
                await ChangeCurrentActionText("Authentication failed");
                break;
            case CoordinatorState.TimedOut:
                await ChangeCurrentActionText("Connection lost. Reconnecting...");
                break;
            case CoordinatorState.Disconnected:
                await ChangeCurrentActionText("Disconnected");
                break;
        }
    }
    
    public async void ToggleFullscreen()
    {
        if (IsFullScreen != WindowState.FullScreen)
            IsFullScreen = WindowState.FullScreen;
        else
            IsFullScreen = WindowState.Normal;
        await ToggleFullscreenMode.Handle(Unit.Default);
    }
}