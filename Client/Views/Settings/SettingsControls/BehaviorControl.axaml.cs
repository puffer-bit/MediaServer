using Avalonia.ReactiveUI;
using Client.Services.Other.AppInfrastructure;
using Client.ViewModels.Settings.SettingsControls;
using Microsoft.Extensions.DependencyInjection;

namespace Client.Views.Settings.SettingsControls;

public partial class BehaviorControl : ReactiveUserControl<BehaviorControlViewModel>
{
    public BehaviorControl()
    {
        InitializeComponent();
    }
}