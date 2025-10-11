using System.Collections.Specialized;
using Shared.Models;

namespace Client.Services.Server.Coordinator;

public partial class CoordinatorSession
{
    public void RaiseSessionAdded(SessionDTO sessionDTO)
    {
        SessionsCollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, sessionDTO));
    }

    public void RaiseSessionRemoved(SessionDTO sessionDTO)
    {
        SessionsCollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, sessionDTO));
    }

    public void RaiseSessionReconfigured(SessionDTO sessionDTO)
    {
        SessionsCollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, sessionDTO));
    }

    public void RaiseHostConnectedToSession(SessionDTO sessionDTO)
    {
        
    }
    
    public void RaiseHostDisconnectedFromSession(SessionDTO sessionDTO)
    {
        
    }
}