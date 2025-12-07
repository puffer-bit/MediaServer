using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using ReactiveUI;

namespace Client.ViewModels;

public class NotificationPopUpViewModel : ReactiveObject
{
    public enum PopUpType
    {
        Info,
        Warning,
        Error,
        Loading,
        Success,
        Question
    }
    private readonly TaskCompletionSource<bool> _tcs = new();

    public Task<bool> Result => _tcs.Task;
    
    private Control? _view;
    
    private string? _tag;
    public string? Tag
    {
        get => _tag;
        set => this.RaiseAndSetIfChanged(ref _tag, value);
    }
    
    private PopUpType _type;
    public PopUpType Type
    {
        get => _type;
        set => this.RaiseAndSetIfChanged(ref _type, value);
    }

    private string? _iconPath;
    public string? IconPath
    {
        get => _iconPath;
        set => this.RaiseAndSetIfChanged(ref _iconPath, value);
    }

    private string? _message;
    public string? Message
    {
        get => _message;
        set => this.RaiseAndSetIfChanged(ref _message, value);
    }

    private IBrush _backgroundColor;
    public IBrush BackgroundColor
    {
        get => _backgroundColor;
        set => this.RaiseAndSetIfChanged(ref _backgroundColor, value);
    }

    private TimeSpan _lifeTime;
    public TimeSpan LifeTime
    {
        get => _lifeTime;
        set => this.RaiseAndSetIfChanged(ref _lifeTime, value);
    }
    
    private bool _isButtonsAvailable;
    public bool IsButtonsAvailable
    {
        get => _isButtonsAvailable;
        set => this.RaiseAndSetIfChanged(ref _isButtonsAvailable, value);
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }
    
    private bool _isClosing;
    public bool IsClosing
    {
        get => _isClosing;
        set => this.RaiseAndSetIfChanged(ref _isClosing, value);
    }

    public ReactiveCommand<Unit, Unit> CloseCommand { get; }
    public ReactiveCommand<Unit, bool> YesCommand { get; }
    public ReactiveCommand<Unit, bool> NoCommand { get; }
    
    public NotificationPopUpViewModel(PopUpType type, string? message, ObservableCollection<NotificationPopUpViewModel> notificationsCollection, string? tag = null)
    {
        CloseCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            IsClosing = true;
            await Task.Delay(200);
            notificationsCollection.Remove(this);
        });
        YesCommand = ReactiveCommand.Create(() => _tcs.TrySetResult(true));
        NoCommand  = ReactiveCommand.Create(() => _tcs.TrySetResult(false));
        
        Type = type;
        Tag = tag;
        Message = message;
        
        switch (type)
        {
            case PopUpType.Warning:
                BackgroundColor = new SolidColorBrush(Color.Parse("#5c4501"));
                IconPath = "avares://Client/Assets/Icons/Warning.svg";
                break;
            
            case PopUpType.Info:
                BackgroundColor = new SolidColorBrush(Color.Parse("#114a78"));
                IconPath = "avares://Client/Assets/Icons/Info.svg";
                break;
            
            case PopUpType.Success:
                BackgroundColor = new SolidColorBrush(Color.Parse("#278a25"));
                IconPath = "avares://Client/Assets/Icons/Completed.svg";
                break;
            
            case PopUpType.Error:
                BackgroundColor = new SolidColorBrush(Color.Parse("#8a2f25"));
                IconPath = "avares://Client/Assets/Icons/Error.svg";
                break;
                        
            case PopUpType.Loading:
                BackgroundColor = new SolidColorBrush(Color.Parse("#363333"));
                IconPath = "avares://Client/Assets/Icons/Info.svg";
                IsLoading = true;
                break;
            
            case PopUpType.Question:
                BackgroundColor = new SolidColorBrush(Color.Parse("#372152"));
                IconPath = "avares://Client/Assets/Icons/Info.svg";
                IsButtonsAvailable = true;
                break;
            
            default:
                BackgroundColor = new SolidColorBrush(Color.Parse("#372152"));
                IconPath = "avares://Client/Assets/Icons/Info.svg";
                IsButtonsAvailable = true;
                break;
        }
    }

    public void SetTag(string tag) => Tag = tag;
    
    public void SetLifetime(TimeSpan lifetime)
    {
        LifeTime = lifetime;
        _ = StartLifetimeCountdown();
    }

    private async Task StartLifetimeCountdown()
    {
        await Task.Delay(LifeTime);
        CloseCommand.Execute();
    }
    
    public void RebuildNotification(PopUpType type, string? message)
    {
        Type = type;

        switch (type)
        {
            case PopUpType.Warning:
                BackgroundColor = new SolidColorBrush(Color.Parse("#5c4501"));
                IconPath = "avares://Client/Assets/Icons/Warning.svg";
                IsButtonsAvailable = false;
                IsLoading = false;
                break;
            
            case PopUpType.Info:
                BackgroundColor = new SolidColorBrush(Color.Parse("#114a78"));
                IconPath = "avares://Client/Assets/Icons/Info.svg";
                IsButtonsAvailable = false;
                IsLoading = false;
                break;
            
            case PopUpType.Success:
                BackgroundColor = new SolidColorBrush(Color.Parse("#278a25"));
                IconPath = "avares://Client/Assets/Icons/Completed.svg";
                IsButtonsAvailable = false;
                IsLoading = false;
                break;
            
            case PopUpType.Error:
                BackgroundColor = new SolidColorBrush(Color.Parse("#8a2f25"));
                IconPath = "avares://Client/Assets/Icons/Error.svg";
                IsButtonsAvailable = false;
                IsLoading = false;
                break;
                        
            case PopUpType.Loading:
                BackgroundColor = new SolidColorBrush(Color.Parse("#363333"));
                IconPath = "avares://Client/Assets/Icons/Info.svg";
                IsButtonsAvailable = false;
                IsLoading = true;
                break;
            
            case PopUpType.Question:
                BackgroundColor = new SolidColorBrush(Color.Parse("#372152"));
                IconPath = "avares://Client/Assets/Icons/Info.svg";
                IsLoading = false;
                IsButtonsAvailable = true;
                break;
            
            default:
                BackgroundColor = new SolidColorBrush(Color.Parse("#372152"));
                IconPath = "avares://Client/Assets/Icons/Info.svg";
                IsButtonsAvailable = true;
                break;
        }
        
        Message = message;
    }
}
