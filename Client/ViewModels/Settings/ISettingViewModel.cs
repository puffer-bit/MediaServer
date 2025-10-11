using System;
using ReactiveUI;

namespace Client.ViewModels.Settings;

public interface ISettingViewModel : IReactiveObject
{
    string Name { get; }

    bool IsExperimentalFeaturesEnabled();
    string? Save();
    void ResetAll();
    bool HasUnsavedSettings();
}