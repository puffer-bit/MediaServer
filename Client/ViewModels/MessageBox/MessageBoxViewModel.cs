using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;

namespace Client.ViewModels.MessageBox;

internal class MessageBoxViewModel : ReactiveObject
{
    public ReactiveCommand<Unit, Result> CancelCommand { get; }
    public ReactiveCommand<Unit, Result> OkCommand { get; }
    public ReactiveCommand<Unit, Result> NoCommand { get; }
    
    public ObservableCollection<ButtonViewModel> ButtonsList { get; } = new();
    public string IconPath { get; set; }
    public Buttons Buttons { get; set; }
    public string Title { get; set; }
    public string Text { get; set; }
    public bool UseButtonsIcons { get; }

    public MessageBoxViewModel(Icon icon, Buttons buttons, string text, string title, bool? useButtonsIcons = false)
    {
        CancelCommand = ReactiveCommand.Create<Unit, Result>(_ => Result.Cancel);
        
        OkCommand = ReactiveCommand.Create<Unit, Result>(_ => Buttons is Buttons.YesNo ? Result.Yes : Result.Ok);
        
        NoCommand = ReactiveCommand.Create<Unit, Result>(_ => Result.No);
        
        switch (icon)
        {
            case MessageBox.Icon.Info:
                IconPath = "avares://Client/Assets/Icons/Info.svg";
                break;
            
            case MessageBox.Icon.Question:
                IconPath = "avares://Client/Assets/Icons/Question.svg";
                break;
            
            case MessageBox.Icon.Warning:
                IconPath = "avares://Client/Assets/Icons/Warning.svg";
                break;
            
            case MessageBox.Icon.Error:
                IconPath = "avares://Client/Assets/Icons/Error.svg";
                break;
            
            case MessageBox.Icon.Fatal:
                IconPath = "avares://Client/Assets/Icons/Stop.svg";
                break;
            
            case MessageBox.Icon.Loading:
                throw new NotImplementedException();
                break;
            
            case MessageBox.Icon.None:
                throw new NotImplementedException();
                break;
            
            default:
                IconPath = "avares://Client/Assets/Icons/Info.svg";
                break;
        }
        Buttons = buttons;
        Text = text;
        Title = title;
        UseButtonsIcons = false;
        LoadMessageBox();
    }

    private void LoadMessageBox()
    {
        ButtonsList.Clear();

        switch (Buttons)
        {
            case Buttons.OkCancel:
                if (UseButtonsIcons)
                {
                    ButtonsList.Add(new ButtonViewModel("Ok", OkCommand, "avares://Client/Assets/Icons/Yes.svg"));
                    ButtonsList.Add(new ButtonViewModel("Cancel", CancelCommand,"avares://Client/Assets/Icons/No.svg"));
                }
                else
                {
                    ButtonsList.Add(new ButtonViewModel("Ok", OkCommand));
                    ButtonsList.Add(new ButtonViewModel("Cancel", CancelCommand));
                }
                break;

            case Buttons.Ok:
                if (UseButtonsIcons)
                    ButtonsList.Add(new ButtonViewModel("Ok", OkCommand, "avares://Client/Assets/Icons/Yes.svg"));
                else
                    ButtonsList.Add(new ButtonViewModel("Ok", OkCommand));
                break;

            case Buttons.YesNo:
                if (UseButtonsIcons)
                {
                    ButtonsList.Add(new ButtonViewModel("Yes", OkCommand, "avares://Client/Assets/Icons/Yes.svg"));
                    ButtonsList.Add(new ButtonViewModel("No", NoCommand, "avares://Client/Assets/Icons/No.svg"));
                }
                else
                {
                    ButtonsList.Add(new ButtonViewModel("Yes", OkCommand));
                    ButtonsList.Add(new ButtonViewModel("No", NoCommand));
                }
                break;
        }
    }
    
    
    private void DoSomething(string name)
    {
        
    }
}