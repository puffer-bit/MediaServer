using System.Reactive;
using System.Reactive.Disposables;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Client.ViewModels.MainWindow.ConnectWindow;
using Client.ViewModels.MessageBox;
using ReactiveUI;

namespace Client.Views.Connections;

internal partial class ConnectWindow : ReactiveWindow<ConnectWindowViewModel>
{
    public ConnectWindow()
    {
        InitializeComponent();

        if (!Design.IsDesignMode)
        {
            this.WhenActivated(disposables =>
            {
                ViewModel!.CloseWindow.RegisterHandler(interaction =>
                {
                    this.Close(interaction.Input);
                    interaction.SetOutput(Unit.Default);
                }).DisposeWith(disposables);
            
                ViewModel!.ShowMessageBox.RegisterHandler(async interaction =>
                {
                    var dialog = new MessageBox.MessageBoxWindow
                    {
                        DataContext = interaction.Input
                    };
                    var result = await dialog.ShowDialog<Result>(this);
                    interaction.SetOutput(result);
                }).DisposeWith(disposables);
            });
        }
        this.Closed += (_, __) =>
        {
            if (DataContext is ConnectWindowViewModel vm)
                vm.Dispose();
        };

    }
}