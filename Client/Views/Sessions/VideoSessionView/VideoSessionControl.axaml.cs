using System;
using System.Reactive;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using Client.ViewModels.MainWindow;
using Client.ViewModels.MessageBox;
using Client.ViewModels.Sessions.VideoSession;
using ReactiveUI;

namespace Client.Views.Chats.VideoSessionView;

internal partial class VideoSessionControl : ReactiveUserControl<VideoSessionViewModel>
{
    public VideoSessionControl()
    {
        InitializeComponent();
        if (!Design.IsDesignMode)
        {
            this.WhenActivated(disposables =>
            {
                ViewModel!.ShowMessageBox.RegisterHandler(async interaction =>
                {
                    if (this.VisualRoot is Window window)
                    {
                        var dialog = new MessageBox.MessageBoxWindow
                        {
                            DataContext = interaction.Input
                        };
                        var result = await dialog.ShowDialog<Result>(window);
                    }
                    interaction.SetOutput(Unit.Default);
                }).DisposeWith(disposables);
            });
            
        }
    }
}