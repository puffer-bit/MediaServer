using System.Reactive;
using System.Reactive.Disposables;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Client.Services.Other.AppInfrastructure;
using Client.Services.Server.Coordinator;
using Client.ViewModels.MainWindow.ConnectWindow;
using Client.ViewModels.MessageBox;
using Client.ViewModels.Settings;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;

namespace Client.Views.Settings;

internal partial class MainSettingsWindow : ReactiveWindow<MainSettingsWindowViewModel>
{
    public MainSettingsWindow()
    {
        InitializeComponent();
        DataContext = new MainSettingsWindowViewModel(App.Services!.GetRequiredService<AppSettingsManager>());
        if (!Design.IsDesignMode)
        {
            this.WhenActivated(disposables =>
            {
                ViewModel!.ShowMessageBox.RegisterHandler(async interaction =>
                {
                    var dialog = new MessageBox.MessageBoxWindow
                    {
                        DataContext = interaction.Input
                    };
                    var result = await dialog.ShowDialog<Result>(this);
                    interaction.SetOutput(result);
                }).DisposeWith(disposables);
                
                ViewModel!.CloseWindow.RegisterHandler(interaction =>
                {
                    this.Close();
                    interaction.SetOutput(Unit.Default);
                }).DisposeWith(disposables);
            });
        }

        this.Closing += (_, _) =>
        {
            App.Services!.GetService<AppSettingsManager>()!.SaveSettings();
        };
    }
}