using System.Collections.Generic;
using Client.Services.Server;

namespace Client.Services.Other.AppInfrastructure;

public class AppSettingsDTO
{
    public bool IsAutoConnectEnabled { get; set; } = true;
    public bool IsUserIdentityEncryptEnabled { get; set; } = false;
    public bool IsUserIdentityEncryptCurrentIdentitiesEnabled { get; set; } = false;
    public string? LastIdentity { get; set; }

    public Dictionary<string, CoordinatorSessionDTO> CoordinatorSessionsIdentities { get; init; }

    public AppSettingsDTO()
    {
        CoordinatorSessionsIdentities = new Dictionary<string, CoordinatorSessionDTO>();
    }
}