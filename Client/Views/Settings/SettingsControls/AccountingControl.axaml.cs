using Avalonia.ReactiveUI;
using Client.ViewModels.Settings.SettingsControls;

namespace Client.Views.Settings.SettingsControls;

public partial class AccountingControl : ReactiveUserControl<AccountingControlViewModel>
{
    public AccountingControl()
    {
        InitializeComponent();
    }
}