using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Client.ViewModels.Sessions;
using FluentAvalonia.UI.Controls;
using Org.BouncyCastle.Crmf;
using Shared.Enums;

namespace Client.Services.UITemplateSelectors;

public class SessionTemplateSelector : IDataTemplate
{
    public bool Match(object? data)
    {
        return data is SessionViewModelWrapper;
    }

    public Control Build(object? data)
    {
        if (data is not SessionViewModelWrapper wrapper)
            return new Grid();
        
        return wrapper.ViewModel.CurrentSessionType switch
        {
            SessionType.Video => new StackPanel
            {
                DataContext = wrapper.ViewModel,
                Children =
                {
                    new NumericUpDown()
                    {
                        Watermark = "Video session capacity",
                        [!NumericUpDown.ValueProperty] = new Binding("Capacity"),
                        Increment = 1,
                        Minimum = 2,
                        [!NumericUpDown.IsHitTestVisibleProperty] = new Binding("!IsLoading")
                    },
                    new CheckBox
                    {
                        Content = "Send audio",
                        [!CheckBox.IsCheckedProperty] = new Binding("IsAudioRequested"),
                        [!CheckBox.IsHitTestVisibleProperty] = new Binding("!IsLoading")
                    }
                }
            },
            SessionType.Voice => new StackPanel
            {
                Children =
                {
                    new TextBlock { Text = "NotImplemented"}
                }
            },
            SessionType.Chat => new StackPanel
            {
                Children =
                {
                    new TextBlock { Text = "NotImplemented"}
                }
            },
            _ => new Grid()
        };
    }
}