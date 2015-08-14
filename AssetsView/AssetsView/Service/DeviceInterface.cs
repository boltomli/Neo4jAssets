using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AssetsView.Service
{
    public interface DeviceInterface
    {
        Assets getDevicesOwnedByEmployee(string msAlias);
    }
}