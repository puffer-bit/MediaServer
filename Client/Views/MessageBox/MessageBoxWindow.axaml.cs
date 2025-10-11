using System.Reactive;
using System.Reactive.Disposables;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Client.ViewModels.MessageBox;
using ReactiveUI;

namespace Client.Views.MessageBox;

internal partial class MessageBoxWindow : ReactiveWindow<MessageBoxViewModel>
{
    public MessageBoxWindow()
    {
        InitializeComponent();

        if (!Design.IsDesignMode)
        {
            this.WhenActivated(disposables =>
            {
                var observerz = Observer.Create<Result>(result => Close((object?)result));
                ViewModel!.OkCommand.Subscribe(observerz);
                var observers = Observer.Create<Result>(result => Close((object?)result));
                ViewModel!.CancelCommand.Subscribe(observers);
                var observera = Observer.Create<Result>(result => Close((object?)result));
                ViewModel!.NoCommand.Subscribe(observera);
            });
        }
    }
}