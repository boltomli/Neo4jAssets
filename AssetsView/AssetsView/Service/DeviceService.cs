using AssetsView.Models;
using AssetsView.ViewModels;
using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace AssetsView.Service
{
    public class DeviceService : DeviceInterface
    {
        private readonly IGraphClient _graphClient;

        public DeviceService(IGraphClient graphClient)
        {
            _graphClient = graphClient;
        }

        public Assets getDevicesOwnedByEmployee(string msAlias)
        {
            Assets assets = new Assets();
            assets.devices = getDevicesOwnedByEmployeeAlias(msAlias);
            return assets;
        }

        private List<MappedDevice> getDevicesOwnedByEmployeeAlias(string msAlias)
        {
            var nd = _graphClient.Cypher
                .Match("(d:Device)<-[:OWNS]-(o:Owner)")
                .Where("o.MSAlias = {MSAlias}")
                .WithParam("MSAlias", msAlias)
                .Return(d => d.As<Node<Device>>())
                .Results;

            List<MappedDevice> mappedDevices = new List<MappedDevice>();

            foreach (var d in nd.ToList())
            {
                string type = "Other device";
                if (!string.IsNullOrWhiteSpace(d.Data.CPU))
                {
                    type = "Computer";
                }

                StringBuilder sb = new StringBuilder();
                sb.Append(d.Data.SN);
                sb.Append(d.Data.Manufacturer);
                sb.Append(d.Data.Model);
                sb.Append(d.Data.WSAsset);
                string desc = sb.ToString();
                MappedDevice mappedDevice = new MappedDevice { owner = msAlias, assetTag = d.Data.AssetTag, description = desc, type = type};
                mappedDevices.Add(mappedDevice);
            }

            return mappedDevices;
        }
    }
}