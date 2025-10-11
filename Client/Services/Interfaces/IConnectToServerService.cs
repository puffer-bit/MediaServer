using System.Threading.Tasks;

namespace Client.Services.Interfaces;

public interface IConnectToServerService
{
    Task<bool> ShowConnectDialogAsync();
}