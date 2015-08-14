using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AssetsView.Service
{
    public interface AssetsInterface
    {
        DeviceInterface deviceInterface { get; set; }
    }
}