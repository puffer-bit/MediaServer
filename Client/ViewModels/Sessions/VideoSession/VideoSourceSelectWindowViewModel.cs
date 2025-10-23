using Client.Services.Other.ScreenCastService.Windows.Win32PortalClient;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using static Client.Services.Other.ScreenCastService.Windows.Win32PortalClient.NativeWindowHelper;

namespace Client.ViewModels.Sessions.VideoSession;

public class VideoSourceSelectWindowViewModel : ReactiveObject
{
    public ObservableCollection<WindowData> Windows { get; } = new();
    private WindowData? _selectedWindow;
    public WindowData? SelectedWindow
    {
        get => _selectedWindow;
        set => this.RaiseAndSetIfChanged(ref _selectedWindow, value);
    }

    public ObservableCollection<MonitorData> Monitors { get; } = new();
    private MonitorData? _selectedMonitor;
    public MonitorData? SelectedMonitor
    {
        get => _selectedMonitor;
        set => this.RaiseAndSetIfChanged(ref _selectedMonitor, value);
    }

    private int _selectedMode = 0;
    public int SelectedMode
    {
        get => _selectedMode;
        set => this.RaiseAndSetIfChanged(ref _selectedMode, value);
    }

    public WindowsScreenCastType? type;

    public Interaction<Unit, Unit> CloseWindow { get; set; }
    public Interaction<Unit, Unit> ShowMessageBox { get; set; }
    public Interaction<(WindowsScreenCastType, string), Unit> ConfirmScreenSources { get; set; }
    public ReactiveCommand<Unit, Unit> CloseCommand { get; }
    public ReactiveCommand<Unit, Unit> GoLive { get; }

    public VideoSourceSelectWindowViewModel()
    {
        CloseWindow = new Interaction<Unit, Unit>();
        ConfirmScreenSources = new Interaction<(WindowsScreenCastType, string), Unit>();
        ShowMessageBox = new Interaction<Unit, Unit>();

        CloseCommand = ReactiveCommand
            .CreateFromTask(async () =>
            {
                await CloseWindow.Handle(Unit.Default);
            },
            outputScheduler: RxApp.MainThreadScheduler);
        GoLive = ReactiveCommand
            .CreateFromTask(async () =>
            {
                if (SelectedMonitor == null && SelectedWindow == null)
                {

                }
                else
                {
                    if (SelectedMode == 0)
                        await ConfirmScreenSources.Handle((WindowsScreenCastType.Window, 
                            SelectedWindow!.Handle.ToString()));
                    else if (SelectedMode == 1)
                        await ConfirmScreenSources.Handle((WindowsScreenCastType.Monitor,
                            SelectedMonitor!.Handle.ToString()));
                    else if (SelectedMode == 2)
                        await ConfirmScreenSources.Handle((WindowsScreenCastType.Device,
                            SelectedWindow!.Handle.ToString()));
                }
            },
            outputScheduler: RxApp.MainThreadScheduler);
    }

    public void LoadWindows()
    {
        Windows.Clear();
        foreach (var window in GetOpenWindows())
        {
            Windows.Add(window);
        }
        foreach (var monitor in GetMonitors())
        {
            Monitors.Add(monitor);
        }
    }

    public void ProceedWindowSelection()
    {

    }
}
