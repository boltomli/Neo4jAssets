using AssetsView.Models;
using AssetsView.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AssetsView.Service
{
    public class Assets
    {
        public Employee employee { get; set; }
        public Device device { get; set; }
        public List<MappedDevice> devices { get; set; }
    }
}