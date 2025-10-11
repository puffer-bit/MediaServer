using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tmds.DBus;

namespace Client.Services.Other.ScreenCastService.XdgDesktopPortalClient;

[DBusInterface("org.freedesktop.portal.Request")]
public interface IRequest : IDBusObject
{
    Task<IDisposable> WatchResponseAsync(Action<(uint response, IDictionary<string, object> results)> handler);
}