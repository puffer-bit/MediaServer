using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.ReactiveUI;
using Client.ViewModels;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;

namespace Client.Views.Notifications;

public partial class NotificationPopUp : ReactiveUserControl<NotificationPopUpViewModel>
{
    public NotificationPopUp()
    {
        this.DataContextChanged += (_, __) =>
        {
            if (DataContext is NotificationPopUpViewModel vm)
            {
                vm.PropertyChanged += async (s, e) =>
                {
                    if (e.PropertyName == nameof(NotificationPopUpViewModel.BackgroundColor))
                    {
                        var newBrush = vm.BackgroundColor as SolidColorBrush;
                        if (newBrush != null)
                        {
                            var oldColor = (this.Background as SolidColorBrush)?.Color ?? Colors.Transparent;
                            var newColor = newBrush.Color;
                            
                            await ChangeBackgroundColor(oldColor, newColor);
                        }
                    }
                    else if (e.PropertyName == nameof(NotificationPopUpViewModel.IsClosing) && vm.IsClosing)
                    {
                        await CloseAnimation();
                    }
                };
            }
        };
        
        this.AttachedToVisualTree += async (_, __) =>
        {
            await AppearAnimation();
        };
        InitializeComponent();
    }

    private async Task AppearAnimation()
    {
        var animation = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(200),
            IterationCount = new IterationCount(1),
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0d),
                    Setters =
                    {
                        new Setter(OpacityProperty, 0d),
                        new Setter(MarginProperty, new Thickness(0, -30, 0, 0))
                    }
                },
                new KeyFrame
                {
                    Cue = new Cue(1d),
                    Setters =
                    {
                        new Setter(OpacityProperty, 1d),
                        new Setter(MarginProperty, new Thickness(0))
                    }
                }
            }
        };
        
        await animation.RunAsync(this, CancellationToken.None);
    }
    
    private async Task ChangeBackgroundColor(Color oldColor, Color newColor)
    {
        var animation = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(200),
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0d),
                    Setters = { new Setter(BackgroundProperty, new SolidColorBrush(oldColor)) }
                },
                new KeyFrame
                {
                    Cue = new Cue(1d),
                    Setters = { new Setter(BackgroundProperty, new SolidColorBrush(newColor)) }
                }
            }
        };

        await animation.RunAsync(this, CancellationToken.None);
    }
    
    private async Task CloseAnimation()
    {
        var animation = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(200),
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame { Cue = new Cue(0d), Setters = { new Setter(Visual.OpacityProperty, 1d) } },
                new KeyFrame { Cue = new Cue(1d), Setters = { new Setter(Visual.OpacityProperty, 0d) } }
            }
        };

        await animation.RunAsync(this, CancellationToken.None);
    }
}