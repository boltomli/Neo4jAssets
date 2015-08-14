using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AssetsView.Service
{
    public class AssetsService : AssetsInterface
    {
        public DeviceInterface deviceInterface { get; set; }
        public AssetsService(DeviceInterface deviceInterface)
        {
            this.deviceInterface = deviceInterface;
        }
    }
}
