using System.Reactive;
using ReactiveUI;

namespace Client.ViewModels.MessageBox;

internal class ButtonViewModel : ReactiveObject
{
    public string Text { get; set; }
    public string? IconPath { get; set; }
    public ReactiveCommand<Unit, Result> Command { get; set; }

    public ButtonViewModel(string text, ReactiveCommand<Unit, Result> command, string? iconPath = null)
    {
        Text = text;
        Command = command;
        if (iconPath != null)
            IconPath = iconPath;
    }
}