using System.Collections.ObjectModel;
using System.Reactive;
using Client.Services.Other.AppInfrastructure;
using Client.Services.Server;
using ReactiveUI;
using Shared.Models.DTO;

namespace Client.ViewModels.Settings.SettingsControls;

public class AccountingControlViewModel : ReactiveObject, ISettingViewModel
{
    public string Name { get; } = "Accounting";
    
    private bool _isUserIdentityEncryptEnabled;
    public bool IsUserIdentityEncryptEnabled
    {
        get => _isUserIdentityEncryptEnabled;
        set
        {
            this.RaiseAndSetIfChanged(ref _isUserIdentityEncryptEnabled, value);
            this.RaisePropertyChanged(nameof(IsExperimentalFeaturesEnabled));
        }
    }
    
    private bool _isUserIdentityEncryptCurrentIdentitiesEnabled;
    public bool IsUserIdentityEncryptCurrentIdentitiesEnabled
    {
        get => _isUserIdentityEncryptCurrentIdentitiesEnabled;
        set
        {
            this.RaiseAndSetIfChanged(ref _isUserIdentityEncryptCurrentIdentitiesEnabled, value);
            this.RaisePropertyChanged(nameof(IsExperimentalFeaturesEnabled));
        }
    }

    private bool _isCollectionChanged { get; set; } = false;

    public ObservableCollection<CoordinatorSessionDTO> Identities { get; } = new ObservableCollection<CoordinatorSessionDTO>();

    public ReactiveCommand<CoordinatorSessionDTO, Unit> DeleteIdentityCommand { get; init; }
    
    private readonly AppSettingsManager _appSettingsManager;
    public AccountingControlViewModel(AppSettingsManager appSettingsManager)
    {
        _appSettingsManager = appSettingsManager;
        _isUserIdentityEncryptEnabled = _appSettingsManager.SettingsData.IsUserIdentityEncryptEnabled;
        
        DeleteIdentityCommand = ReactiveCommand
            .Create<CoordinatorSessionDTO>(DeleteIdentity,
                outputScheduler: RxApp.MainThreadScheduler);

        foreach (var identity in _appSettingsManager.SettingsData.CoordinatorSessionsIdentities)
        {
            Identities.Add(identity.Value);
        }
        
        Identities.CollectionChanged += (_, __) =>
        {
            _isCollectionChanged = true;
            this.RaisePropertyChanged();
        };
    }
    
    public string? Save()
    {
        _appSettingsManager.CommitIdentityChanges(Identities);
        _appSettingsManager.SettingsData.IsUserIdentityEncryptEnabled = IsUserIdentityEncryptEnabled;
        if (IsUserIdentityEncryptCurrentIdentitiesEnabled)
            _appSettingsManager.SettingsData.IsUserIdentityEncryptCurrentIdentitiesEnabled = IsUserIdentityEncryptCurrentIdentitiesEnabled;
        return null;
    }

    public void ResetAll()
    {
        Identities.Clear();
        foreach (var identity in _appSettingsManager.SettingsData.CoordinatorSessionsIdentities)
        {
            Identities.Add(identity.Value);
        }
    }
    
    public bool HasUnsavedSettings()
    {
        if (_appSettingsManager.SettingsData.IsUserIdentityEncryptEnabled != IsUserIdentityEncryptEnabled)
            return true;
        if (_appSettingsManager.SettingsData.IsUserIdentityEncryptCurrentIdentitiesEnabled != IsUserIdentityEncryptCurrentIdentitiesEnabled)
            return true;
        if (_isCollectionChanged)
            return true;
        return false;
    }

    private void DeleteIdentity(CoordinatorSessionDTO coordinatorSessionDTO)
    {
        Identities.Remove(coordinatorSessionDTO);
    }

    public bool IsExperimentalFeaturesEnabled()
    {
        if (IsUserIdentityEncryptEnabled)
            return true;
        return false;
    }
}