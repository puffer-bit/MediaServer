using System;
using System.Collections;
using System.IO;
using System.Text.Json;
using Client.Services.Server;
using Newtonsoft.Json;
using ReactiveUI;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace Client.Services.Other.AppInfrastructure;

public class AppSettingsManager : ReactiveObject
{
    private static string _settingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Beta_Client",
        "settings.json");
    public bool IsInitialized { get; set; }
    public AppSettingsDTO SettingsData { get; set; }
    
    public AppSettingsManager()
    {
        if (File.Exists(_settingsPath))
        {
            var json = File.ReadAllText(_settingsPath);
            AppSettingsDTO? dto = JsonConvert.DeserializeObject<AppSettingsDTO>(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
            if (dto == null)
            {
                SettingsData = new AppSettingsDTO();
                IsInitialized = true;
                return;
            }

            IsInitialized = true;
            SettingsData = dto;
        }
        else
        {
            SettingsData = new AppSettingsDTO();
            IsInitialized = true;
        }
    }

    public void SetCoordinatorForAutoConnect(CoordinatorSessionDTO coordinatorSessionDTO)
    {
        SettingsData.LastIdentity = coordinatorSessionDTO.User.Id;
    }

    public void AddIdentity(string userId, CoordinatorSessionDTO coordinatorSessionDTO)
    {
        SettingsData.CoordinatorSessionsIdentities.TryAdd(userId, coordinatorSessionDTO);
    }
    
    public void DeleteIdentity(CoordinatorSessionDTO coordinatorSessionDTO)
    {
        if (coordinatorSessionDTO.User.Id == null)
            return;
        SettingsData.CoordinatorSessionsIdentities.Remove(coordinatorSessionDTO.User.Id);
    }

    public void CommitIdentityChanges(ICollection collection)
    {
        SettingsData.CoordinatorSessionsIdentities.Clear();

        foreach (var item in collection)
        {
            if (item is CoordinatorSessionDTO dto && dto.User.Id != null)
            {
                SettingsData.CoordinatorSessionsIdentities[dto.User.Id] = dto;
            }
        }
    }

    public void LoadSettings()
    {
        
    }
    
    public void SaveSettings()
    {
        var json = JsonConvert.SerializeObject(SettingsData, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });

        var directory = Path.GetDirectoryName(_settingsPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(_settingsPath, json);
    }
}