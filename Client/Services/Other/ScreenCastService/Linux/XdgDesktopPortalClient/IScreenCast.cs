using System.Collections.Generic;
using System.Threading.Tasks;
using Tmds.DBus;

namespace Client.Services.Other.ScreenCastService.XdgDesktopPortalClient;

[DBusInterface("org.freedesktop.portal.ScreenCast")]
public interface IScreenCast : IDBusObject
{
    Task<ObjectPath> CreateSessionAsync(IDictionary<string, object> options);
    Task<ObjectPath> SelectSourcesAsync(ObjectPath session, IDictionary<string, object> options);
    Task<ObjectPath> StartAsync(ObjectPath session, string parentWindow, IDictionary<string, object> options);
}