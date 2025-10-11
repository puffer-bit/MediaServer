using System;
using System.Reactive;
using System.Reactive.Disposables;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Client.Services.Server.Coordinator;
using Client.ViewModels.MainWindow;
using Client.ViewModels.MainWindow.ConnectWindow;
using Client.ViewModels.MessageBox;
using Client.Views.Chats.CreateSessionWindow;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;

namespace Client.Views;

internal partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();
        if (!Design.IsDesignMode)
        {
            this.WhenActivated(disposables =>
            {
                ViewModel!.ShowConnectWindow.RegisterHandler(async interaction =>
                {
                    var vm = App.Services!.GetRequiredService<ConnectWindowViewModel>();
                    var dialog = new Connections.ConnectWindow
                    {
                        DataContext = vm
                    };

                    var result = await dialog.ShowDialog<CoordinatorSession>(this);
                    
                    (vm as IDisposable).Dispose();

                    interaction.SetOutput(result);
                }).DisposeWith(disposables);
            
                ViewModel!.ShowSettings.RegisterHandler(async interaction =>
                {
                    var dialog = new Settings.MainSettingsWindow();
                    await dialog.ShowDialog<CoordinatorSession>(this);
                    interaction.SetOutput(Unit.Default);
                }).DisposeWith(disposables);
                
                ViewModel!.ShowTestBox.RegisterHandler(async interaction =>
                {
                    var dialog = new MessageBox.MessageBoxWindow
                    {
                        DataContext = interaction.Input
                    };
                    var result = await dialog.ShowDialog<Result>(this);
                    interaction.SetOutput(result);
                }).DisposeWith(disposables);
                
                ViewModel!.ShowCreateSessionWindow.RegisterHandler(async interaction =>
                {
                    var dialog = new CreateSessionWindow
                    {
                        DataContext = interaction.Input
                    };
                    await dialog.ShowDialog<Unit>(this);
                    interaction.SetOutput(Unit.Default);
                }).DisposeWith(disposables);
                
                ViewModel!.ToggleFullscreenMode.RegisterHandler(interaction =>
                {
                    if (ViewModel!.IsFullScreen == WindowState.FullScreen)
                    {
                        Grid.SetColumn(MainContentControl, 0);
                        Grid.SetColumnSpan(MainContentControl, 3);
                    }
                    else
                    {
                        Grid.SetColumn(MainContentControl, 2);
                        Grid.SetColumnSpan(MainContentControl, 1);
                    }
                        
                    interaction.SetOutput(Unit.Default);
                }).DisposeWith(disposables);
            });
        }
    }
}