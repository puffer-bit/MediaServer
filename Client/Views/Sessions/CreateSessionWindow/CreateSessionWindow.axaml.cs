using System.Reactive;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Client.ViewModels.MessageBox;
using Client.ViewModels.Sessions;
using ReactiveUI;

namespace Client.Views.Chats.CreateSessionWindow;

internal partial class CreateSessionWindow : ReactiveWindow<CreateSessionWindowViewModel>
{
    public CreateSessionWindow()
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
                    interaction.SetOutput(Unit.Default);
                }).DisposeWith(disposables);
            });
        }
    }
}