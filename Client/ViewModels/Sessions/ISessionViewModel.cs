using System;
using System.Threading.Tasks;

namespace Client.ViewModels.Sessions;

public interface ISessionViewModel : IDisposable
{
    string Id { get; }
    string Name { get; }

    event Action? RequestClose;
    event Action? RequestFullScreen;
    
    Task JoinSession();
    void CloseSession();
}