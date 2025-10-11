using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Client.Services.Other.AppInfrastructure;
using Client.ViewModels.MessageBox;
using Client.ViewModels.Settings.SettingsControls;
using DynamicData.Binding;
using ReactiveUI;

namespace Client.ViewModels.Settings;

internal class MainSettingsWindowViewModel : ReactiveObject
{
    public ObservableCollection<ISettingViewModel> SettingList { get; } = new ObservableCollection<ISettingViewModel>();
    
    public ReactiveCommand<Unit, Unit> SaveCommand { get; init; }
    public ReactiveCommand<Unit, Unit> CloseCommand { get; init; }
    public ReactiveCommand<Unit, Unit> ResetAllSettingsCommand { get; init; }
    
    public Interaction<Unit, Unit> CloseWindow { get; init; }
    public Interaction<MessageBoxViewModel, Result> ShowMessageBox { get; init; }
    
    private ISettingViewModel? _selectedSetting;
    public ISettingViewModel? SelectedSetting
    {
        get => _selectedSetting;
        set => this.RaiseAndSetIfChanged(ref _selectedSetting, value);
    }

    private bool _isEdited;
    public bool IsEdited
    {
        get => _isEdited;
        set => this.RaiseAndSetIfChanged(ref _isEdited, value);
    }
    
    private bool _isReset;
    public bool IsReset
    {
        get => _isReset;
        set => this.RaiseAndSetIfChanged(ref _isReset, value);
    }
    
    private bool _isExperimentalFeaturesEnabled;
    public bool IsExperimentalFeaturesEnabled
    {
        get => _isExperimentalFeaturesEnabled;
        set => this.RaiseAndSetIfChanged(ref _isExperimentalFeaturesEnabled, value);
    }
    
    public MainSettingsWindowViewModel(AppSettingsManager appSettingsManager, string? selectedSetting = null)
    {
        CloseWindow = new Interaction<Unit, Unit>();
        ShowMessageBox = new Interaction<MessageBoxViewModel, Result>();
        
        SaveCommand = ReactiveCommand
            .CreateFromTask(SaveAsync,
                outputScheduler: RxApp.MainThreadScheduler);
        CloseCommand = ReactiveCommand
            .CreateFromTask(CloseAsync,
                outputScheduler: RxApp.MainThreadScheduler);
        ResetAllSettingsCommand = ReactiveCommand
            .CreateFromTask(ResetAllSettings,
                outputScheduler: RxApp.MainThreadScheduler);
        
        SettingList.Add(new AccountingControlViewModel(appSettingsManager));
        SettingList.Add(new BehaviorControlViewModel(appSettingsManager));
        foreach (var setting in SettingList)
        {
            setting.WhenAnyPropertyChanged()
                .Subscribe(_ => OnSettingChanged(setting));
        }
        
        // Check for experimental features
        foreach (var setting in SettingList)
        {
            if (setting.IsExperimentalFeaturesEnabled())
            {
                IsExperimentalFeaturesEnabled = true;
                break;
            }
        }
        
        if (selectedSetting == null)
        {
            SelectedSetting = SettingList[0];
        }
    }

    private async Task SaveAsync()
    {
        if (IsExperimentalFeaturesEnabled)
        {
            if (await ShowMessageBox.Handle(new MessageBoxViewModel(Icon.Warning, Buttons.YesNo, 
                    "Using experimental settings may result in unstable operation. Are you sure you want to use these settings?", "Warning")) == Result.No)
            {
                return;
            }
        }
        
        string failedSettings = "";
        
        foreach (var setting in SettingList)
        {
            string? result = setting.Save();
            if (result != null)
            {
                failedSettings += $"• {result}\n";
                break;
            }
        }
        if (failedSettings != "")
        {
            await ShowMessageBox.Handle(new MessageBoxViewModel(Icon.Error, Buttons.Ok,
                $"The following settings could not be saved: \n {failedSettings} \nParameters with errors will be skipped.", "Error"));
        }
        await CloseWindow.Handle(Unit.Default);
    }
    
    private async Task CloseAsync()
    {
        if (IsEdited)
        {
            if (await ShowMessageBox.Handle(new MessageBoxViewModel(Icon.Question, Buttons.OkCancel, 
                    "Some of settings not saved yet. Continue without saving?", "Question", false)) == Result.Ok)
            {
                await CloseWindow.Handle(Unit.Default);
            }
        }
        else
            await CloseWindow.Handle(Unit.Default);
    }

    private async Task ResetAllSettings()
    {
        if (await ShowMessageBox.Handle(new MessageBoxViewModel(Icon.Warning, Buttons.YesNo, 
                "This action will reset all app settings. This cannot be undone. Are you sure you want to proceed?", "Warning", false)) == Result.Yes)
        {
            foreach (var setting in SettingList)
            {
                setting.ResetAll();
            }
            IsReset = true;
        }
    }
    
    private void OnSettingChanged(ISettingViewModel setting)
    {
        IsReset = false;
        
        if (setting.HasUnsavedSettings())
            IsEdited = true;
        else
            IsEdited = false;

        if (setting.IsExperimentalFeaturesEnabled())
            IsExperimentalFeaturesEnabled = true;
        else
            IsExperimentalFeaturesEnabled = false;
    }
}