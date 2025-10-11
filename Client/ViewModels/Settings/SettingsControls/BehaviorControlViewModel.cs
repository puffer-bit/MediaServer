using System;
using System.Reactive;
using Client.Services.Other.AppInfrastructure;
using ReactiveUI;

namespace Client.ViewModels.Settings.SettingsControls;

public class BehaviorControlViewModel : ReactiveObject, ISettingViewModel
{
    public string Name { get; } = "Behavior";

    private bool _isAutoConnectionEnabled;
    public bool IsAutoConnectionEnabled
    {
        get => _isAutoConnectionEnabled;
        set
        {
            this.RaiseAndSetIfChanged(ref _isAutoConnectionEnabled, value);
            this.RaisePropertyChanged(nameof(IsExperimentalFeaturesEnabled));
        }
    }
    
    public bool IsCoordinatorDTOReset => _appSettingsManager.SettingsData.LastIdentity == null;
    
    private readonly AppSettingsManager _appSettingsManager;
    
    public ReactiveCommand<Unit, Unit> ResetCoordinatorDTOCommand { get; init; }
    
    public BehaviorControlViewModel(AppSettingsManager appSettingsManager)
    {
        _appSettingsManager = appSettingsManager;
        _isAutoConnectionEnabled = appSettingsManager.SettingsData.IsAutoConnectEnabled;

        ResetCoordinatorDTOCommand = ReactiveCommand
            .Create(ResetLastIdentity,
            outputScheduler: RxApp.MainThreadScheduler);
    }
    
    public string? Save()
    {
        _appSettingsManager.SettingsData.IsAutoConnectEnabled = IsAutoConnectionEnabled;
        try
        {
            _appSettingsManager.SaveSettings();
        }
        catch (Exception ex)
        {
            return "Not excepted error in Behavior settings.";
        }
        return null;
    }

    public void ResetAll()
    {
        IsAutoConnectionEnabled = true;
        Save();
    }
    
    public bool HasUnsavedSettings()
    {
        if (IsAutoConnectionEnabled == _appSettingsManager.SettingsData.IsAutoConnectEnabled)
        {
            return false;
        }

        return true;
    }
    
    private void ResetLastIdentity()
    {
        _appSettingsManager.SettingsData.LastIdentity = null;
        this.RaisePropertyChanged(nameof(IsCoordinatorDTOReset));
        _appSettingsManager.SaveSettings();
    }
    
    public bool IsExperimentalFeaturesEnabled()
    {
        if (IsAutoConnectionEnabled)
            return true;
        return false;
    }
}