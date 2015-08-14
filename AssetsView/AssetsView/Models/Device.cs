using Neo4jClient;

namespace AssetsView.Models
{
    public class Device
    {
        public long nodeId { get; set; }
        public NodeReference noderef { get; set; }
        public string WSAsset { get; set; }
        public string AssetTag { get; set; }
        public string Manufacturer { get; set; }
        public string SN { get; set; }
        public string Model { get; set; }
        public string CPU { get; set; }
        public string RAM { get; set; }
        public string Disk { get; set; }
        public string SystemDate { get; set; }
        public string OS { get; set; }
        public string OSInstallDate { get; set; }
    }
}