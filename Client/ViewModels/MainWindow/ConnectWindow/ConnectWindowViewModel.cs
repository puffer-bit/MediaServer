using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using Client.Services.Other.AppInfrastructure;
using Client.Services.Server.Coordinator;
using Client.ViewModels.MessageBox;
using ReactiveUI;
using Shared.Enums;
using Shared.Models;
using Shared.Models.DTO;

namespace Client.ViewModels.MainWindow.ConnectWindow;

internal class ConnectWindowViewModel : ReactiveObject, IDisposable
{
    public ObservableCollection<ServerType> SupportedServers { get; } = new()
    {
        ServerType.WebSocket, ServerType.WebSocketSecure, ServerType.SignalR
    };
    
    public ObservableCollection<UserDTO> Identities { get; } = new ObservableCollection<UserDTO>();
    private UserDTO? _selectedIdentity;
    public UserDTO? SelectedIdentity
    {
        get => _selectedIdentity;
        set => this.RaiseAndSetIfChanged(ref _selectedIdentity, value);
    }
    public bool IsIdentitiesSelectable => Identities.Count > 1;
    private readonly CancellationTokenSource _cancellationTokenSource = new();    
    
    private bool _isConnecting;
    public bool IsConnecting
    {
        get => _isConnecting;
        set => this.RaiseAndSetIfChanged(ref _isConnecting, value);
    }
    
    private bool _isPasswordVisible;
    public bool IsPasswordVisible
    {
        get => _isPasswordVisible;
        set
        {
            this.RaiseAndSetIfChanged(ref _isPasswordVisible, value);
            this.RaisePropertyChanged(nameof(PasswordChar));
        }
    }

    private string _connectionStatus = "Waiting for action...";
    public string ConnectionStatus
    {
        get => _connectionStatus;
        set => this.RaiseAndSetIfChanged(ref _connectionStatus, value);
    }
    
    private string _serverAddress = "localhost:26666";
    public string ServerAddress
    {
        get => _serverAddress;
        set
        {
            this.RaiseAndSetIfChanged(ref _serverAddress, value);
            FindIdentities();
            this.RaisePropertyChanged(nameof(IsIdentitiesSelectable));
        }
    }

    public string PasswordChar => IsPasswordVisible ? "" : "*";
    
    private string? _password;
    public string? Password
    {
        get => _password;
        set => this.RaiseAndSetIfChanged(ref _password, value);
    }
    
    private ServerType _serverType = ServerType.WebSocketSecure;
    public ServerType ServerType
    {
        get => _serverType;
        set => this.RaiseAndSetIfChanged(ref _serverType, value);
    }
    
    public Interaction<ICoordinatorSession?, Unit> CloseWindow { get; }
    public Interaction<MessageBoxViewModel, Result> ShowMessageBox { get; }

    public ReactiveCommand<Unit, Unit> ConnectCommand { get; }
    public ReactiveCommand<Unit, Unit> CloseCommand { get; }

    private readonly AppSettingsManager _appSettingsManager;

    public ConnectWindowViewModel(ICoordinatorSession coordinatorSession, AppSettingsManager appSettingsManager)
    {
        _appSettingsManager = appSettingsManager;

        CloseWindow = new();
        ShowMessageBox = new ();

        ConnectCommand = ReactiveCommand.CreateFromTask(async cancellationToken =>
        {
            IsConnecting = true;

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                ConnectionStatus = "Waiting for server reply...";

                if (SelectedIdentity == null || SelectedIdentity.Id == null)
                    await coordinatorSession.ConnectAndAuthenticate(new UserDTO("User"), ServerAddress, cancellationToken);
                else
                    await coordinatorSession.ConnectAndAuthenticate(SelectedIdentity, ServerAddress);

                cancellationToken.ThrowIfCancellationRequested();

                if (coordinatorSession.ConnectionStatus != CoordinatorState.Connected)
                {
                    await ShowMessageBox.Handle(new MessageBoxViewModel(Icon.Error, Buttons.Ok,
                        "Connection to server failed. \n\nMake sure the server is available and check the entered data. ",
                        "Error", true));
                    IsConnecting = false;
                    return;
                }

                if (SelectedIdentity == null || SelectedIdentity.Id == null)
                {
                    var coordinatorSessionDto = coordinatorSession.AsModel();
                    _appSettingsManager.AddIdentity(coordinatorSessionDto!.User.Id!, coordinatorSessionDto);
                    _appSettingsManager.SetCoordinatorForAutoConnect(coordinatorSessionDto);
                    _appSettingsManager.SaveSettings();
                }

                await CloseWindow.Handle(coordinatorSession);
            }
            catch (OperationCanceledException)
            {
                IsConnecting = false;
            }
            catch (TimeoutException ex)
            {
                IsConnecting = false;
                ConnectionStatus = "Waiting for action...";
                await ShowMessageBox.Handle(new MessageBoxViewModel(Icon.Error, Buttons.Ok, "Connection to server failed. \n\nConnection timed out. ", 
                    "Error", true));
            }
        },
            this
                .WhenAnyValue(x => x.IsConnecting)
                .Select(connecting => !connecting),
            outputScheduler: RxApp.MainThreadScheduler
        );
        
        CloseCommand = ReactiveCommand.
            CreateFromTask(async () =>
            {
                await CloseWindow.Handle(null);
            },
            outputScheduler: RxApp.MainThreadScheduler);
        FindIdentities();
    }

    private void FindIdentities()
    {
        Identities.Clear();
        Identities.Add(new UserDTO("New*"));
        
        var matches = _appSettingsManager.SettingsData.CoordinatorSessionsIdentities
            .Values
            .Where(coordinatorSessionDTO => coordinatorSessionDTO.Address == ServerAddress)
            .ToList();

        foreach (var match in matches)
        {
            Identities.Add(match.User);
            if (match.User.Id == _appSettingsManager.SettingsData.LastIdentity)
            {
                SelectedIdentity = Identities[Identities.Count - 1];
            }
        }

        if (SelectedIdentity == null)
        {
            SelectedIdentity = Identities[0];
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        ConnectCommand.Dispose();
        CloseCommand.Dispose();
    }
}