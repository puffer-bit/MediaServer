using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Client.Services.Server.Coordinator;
using Client.ViewModels.MessageBox;
using ReactiveUI;
using Shared.Enums;
using Shared.Models;

namespace Client.ViewModels.Sessions;

internal class CreateSessionWindowViewModel : ReactiveObject, IDisposable
{
    private readonly ICoordinatorSession _coordinatorSession;
    public bool IsEditMode { get; set; }
    
    public ObservableCollection<SessionType> SessionTypes { get; } = new ObservableCollection<SessionType>(
        Enum.GetValues<SessionType>().Where(x => x != SessionType.Undefiend));
    
    private SessionViewModelWrapper? _dynamicContent;
    public SessionViewModelWrapper? DynamicContent
    {
        get => _dynamicContent;
        set => this.RaiseAndSetIfChanged(ref _dynamicContent, value);
    }

    private SessionType _currentSessionType = SessionType.Video;
    public SessionType CurrentSessionType
    {
        get => _currentSessionType;
        set
        {
            this.RaiseAndSetIfChanged(ref _currentSessionType, value);
            DynamicContent = new SessionViewModelWrapper(this);
        }
    }
    
    private string? _title;
    public string? Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }
    
    private string? _name;
    public string? Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }
    
    private string? _currentAction;
    public string? CurrentAction
    {
        get => _currentAction;
        set => this.RaiseAndSetIfChanged(ref _currentAction, value);
    }
    
    private int _capacity;
    public int Capacity
    {
        get => _capacity;
        set => this.RaiseAndSetIfChanged(ref _capacity, value);
    }

    private bool _isAudioRequested;
    public bool IsAudioRequested
    {
        get => _isAudioRequested;
        set => this.RaiseAndSetIfChanged(ref _isAudioRequested, value);
    }
    
    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }
    
    private double _opacity = 1;
    public double Opacity
    {
        get => _opacity;
        set => this.RaiseAndSetIfChanged(ref _opacity, value);
    }
    
    private bool _opacityLoading;
    public bool OpacityLoading
    {
        get => _opacityLoading;
        set => this.RaiseAndSetIfChanged(ref _opacityLoading, value);
    }

    public Interaction<Unit, Unit> CloseWindow { get; set; }
    public Interaction<MessageBoxViewModel, Unit> ShowMessageBox { get; set; }
    
    public ReactiveCommand<Unit, Unit> CloseCommand { get; set; }
    public ReactiveCommand<Unit, Unit> CreateCommand { get; set; }
    public ReactiveCommand<Unit, Unit> EditCommand { get; set; }

    public CreateSessionWindowViewModel(ICoordinatorSession coordinatorSession, 
        SessionType currentSessionType, 
        bool isEditMode)
    {
        _coordinatorSession = coordinatorSession;
        _currentSessionType = currentSessionType;
        DynamicContent = new SessionViewModelWrapper(this); // Cant without this(
        IsEditMode = isEditMode;

        CloseWindow = new Interaction<Unit, Unit>();
        ShowMessageBox = new Interaction<MessageBoxViewModel, Unit>();
        
        CreateCommand = ReactiveCommand
            .CreateFromTask(TryCreateSession,
            outputScheduler: RxApp.MainThreadScheduler);
        
        EditCommand = ReactiveCommand
            .Create(() =>
            {
                _ = Task.Run(async () =>
                {
                    await TryEditSession();
                });
            },
            outputScheduler: RxApp.MainThreadScheduler);
        
        CloseCommand = ReactiveCommand
            .CreateFromTask(async () =>
            {
                await CloseWindow.Handle(Unit.Default);
            },
            outputScheduler: RxApp.MainThreadScheduler);
        
        // UI Init
        if (isEditMode)
        {
            Title = "Edit session";
        }
        else
        {
            Title = "Create session";
        }
    }

    private async Task TryCreateSession()
    {
        StartLoading();
        await Task.Delay(1500);
        var result = await _coordinatorSession.CreateSession(new VideoSessionDTO()
        {
            Id = "-1",
            Name = this.Name!,
            Capacity = this.Capacity,
            IsAudioRequested = this.IsAudioRequested,
            HostId = _coordinatorSession.GetUser().Id,
            SessionType = CurrentSessionType,
        });
        EndLoading();
        
        if (result == CreateSessionResult.UnexceptedParameters)
        {
            await ShowMessageBox.Handle(new MessageBoxViewModel(Icon.Error, Buttons.Ok,
                $"Failed to create session. \n\nWrong data received. Check all fields and retry. \n\nCode: {(int)result}", "Error",
                false));
        }
        else if (result != CreateSessionResult.NoError)
        {
            await ShowMessageBox.Handle(new MessageBoxViewModel(Icon.Error, Buttons.Ok,
                $"Failed to create session. \n\nAn unexpected error occurred. \n\nCode: {(int)result} ", "Error",
                false));
        }
        else
            await CloseWindow.Handle(Unit.Default);
    }

    private async Task TryEditSession()
    {
        throw new NotImplementedException();
    }
    
    private void StartLoading()
    {
        Opacity = 0.5;
        CurrentAction = IsEditMode ? "Saving session..." : "Creating session...";
        OpacityLoading = true;
        IsLoading = true;
    }

    private void EndLoading()
    {
        OpacityLoading = false;
        IsLoading = false;
        Opacity = 1;
    }

    public void Dispose()
    {
        CloseCommand.Dispose();
        CreateCommand.Dispose();
        EditCommand.Dispose();
    }
}

internal class SessionViewModelWrapper
{
    public CreateSessionWindowViewModel ViewModel { get; }

    public SessionViewModelWrapper(CreateSessionWindowViewModel vm)
    {
        ViewModel = vm;
    }
}
