using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AssetsView.ViewModels
{
    public class MappedDevice
    {
        public string owner { get; set; }
        public string assetTag { get; set; }
        public string type { get; set; }
        public string description { get; set; }
    }
}