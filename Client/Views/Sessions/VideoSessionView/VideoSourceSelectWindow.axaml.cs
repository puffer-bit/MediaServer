using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Client.ViewModels.Sessions.VideoSession;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Disposables;

namespace Client.Views.Sessions.VideoSessionView;

public partial class VideoSourceSelectWindow : ReactiveWindow<VideoSourceSelectWindowViewModel>
{
    public VideoSourceSelectWindow()
    {
        InitializeComponent();
        var vm = new VideoSourceSelectWindowViewModel();
        DataContext = vm;
        vm.LoadWindows();
        if (!Design.IsDesignMode)
        {
            this.WhenActivated(disposables =>
            {
                ViewModel!.CloseWindow.RegisterHandler(interaction =>
                {
                    this.Close(interaction.Input);
                    interaction.SetOutput(Unit.Default);
                }).DisposeWith(disposables);
                ViewModel!.ConfirmScreenSources.RegisterHandler(interaction =>
                {
                    this.Close(interaction.Input);
                    interaction.SetOutput(Unit.Default);
                }).DisposeWith(disposables);
            });
        }
    }
}