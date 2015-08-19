using Neo4jClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetsMan
{
    public class Owner
    {
        public string MSAlias { get; set; }
        public string WSAlias { get; set; }
        public string Email { get; set; }
    }

    public class Device
    {
        public string SN { get; set; }
        public string WSAsset { get; set; }
        public string AssetTag { get; set; }
        public string Model { get; set; }
        public string Manufacturer { get; set; }
        public string CPU { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            //UpdateReportsTo("relationship.txt");
            //UpdateAssetTag("assettag.txt");
            //CleanupProperties();
            //CreateType();
        }

        private static void CreateType()
        {
            var client = new GraphClient(new Uri("http://neo4j:111111@localhost:7474/db/data"));
            client.Connect();

            var data = client.Cypher
                .Match("(device:Device)")
                .Return(device => device.As<Device>())
                .Results.ToList();

            foreach (var result in data)
            {
                if (result.Manufacturer.ToLower().Contains("apple"))
                {
                    client.Cypher
                        .Match("(device:Device {SN: {sn}})")
                        .Set("device.Type = {type}")
                        .WithParams(new
                        {
                            sn = result.SN,
                            type = "Mac"
                        })
                        .ExecuteWithoutResults();
                }
                else if (!string.IsNullOrWhiteSpace(result.CPU))
                {
                    if (result.Manufacturer.ToLower().Contains("lenovo"))
                    {
                        client.Cypher
                            .Match("(device:Device {SN: {sn}})")
                            .Set("device.Type = {type}")
                            .WithParams(new
                            {
                                sn = result.SN,
                                type = "Laptop"
                            })
                            .ExecuteWithoutResults();
                    }
                    else
                    {
                        client.Cypher
                            .Match("(device:Device {SN: {sn}})")
                            .Set("device.Type = {type}")
                            .WithParams(new
                            {
                                sn = result.SN,
                                type = "Desktop"
                            })
                            .ExecuteWithoutResults();
                    }
                }
                else if (result.Manufacturer.ToLower().Contains("del") && result.SN.Length > 7)
                {
                    client.Cypher
                        .Match("(device:Device {SN: {sn}})")
                        .Set("device.Type = {type}")
                        .WithParams(new
                        {
                            sn = result.SN,
                            type = "Monitor"
                        })
                        .ExecuteWithoutResults();
                }
                else
                {
                    client.Cypher
                        .Match("(device:Device {SN: {sn}})")
                        .Set("device.Type = {type}")
                        .WithParams(new
                        {
                            sn = result.SN,
                            type = "Misc"
                        })
                        .ExecuteWithoutResults();
                }
            }
        }

        private static void CleanupProperties()
        {
            var client = new GraphClient(new Uri("http://neo4j:111111@localhost:7474/db/data"));
            client.Connect();

            var data = client.Cypher
                .Match("(device:Device)")
                .Return(device => device.As<Device>())
                .Results.ToList();

            foreach (var result in data)
            {
                if (result.SN != result.SN.Trim())
                {
                    client.Cypher
                        .Match("(device:Device {SN: {sn}})")
                        .Set("device.SN = {trimsn}")
                        .WithParams(new
                        {
                            sn = result.SN,
                            trimsn = result.SN.Trim()
                        })
                        .ExecuteWithoutResults();
                }
                if (result.AssetTag != result.AssetTag.Trim())
                {
                    client.Cypher
                        .Match("(device:Device {AssetTag: {asset}})")
                        .Set("device.AssetTag = {trimasset}")
                        .WithParams(new
                        {
                            asset = result.AssetTag,
                            trimasset = result.AssetTag.Trim()
                        })
                        .ExecuteWithoutResults();
                }
                if (result.WSAsset != result.WSAsset.Trim())
                {
                    client.Cypher
                        .Match("(device:Device {WSAsset: {wsasset}})")
                        .Set("device.WSAsset = {trimwsasset}")
                        .WithParams(new
                        {
                            wsasset = result.WSAsset,
                            trimwsasset = result.WSAsset.Trim()
                        })
                        .ExecuteWithoutResults();
                }
            }
        }

        private static void UpdateReportsTo(string file)
        {
            Dictionary<string, string> employees = new Dictionary<string, string>();

            StreamReader sr = new StreamReader(file, true);
            while (!sr.EndOfStream)
            {
                string[] line = sr.ReadLine().Split('\t');
                employees.Add(line[0], line[1]);
            }

            var client = new GraphClient(new Uri("http://neo4j:111111@localhost:7474/db/data"));
            client.Connect();

            foreach (string employee in employees.Keys)
            {
                // Prepare employee
                string msAlias = employee;
                string wsAlias = msAlias.Substring(2, msAlias.Length - 2);
                Owner newEmployee = new Owner
                {
                    MSAlias = msAlias,
                    WSAlias = wsAlias,
                    Email = msAlias + "@ms;" + wsAlias + "@ws"
                };

                // Employee must exist already
                //client.Cypher
                //    .Merge("(owner:Owner { MSAlias: {msalias} })")
                //    .OnCreate()
                //    .Set("owner = {newEmployee}")
                //    .WithParams(new
                //    {
                //        msalias = newEmployee.MSAlias,
                //        newEmployee
                //    })
                //    .ExecuteWithoutResults();

                // Prepare manager
                msAlias = employees[employee];
                wsAlias = msAlias.Substring(2, msAlias.Length - 2);
                Owner newManager = new Owner
                {
                    MSAlias = msAlias,
                    WSAlias = wsAlias,
                    Email = msAlias + "@ms;" + wsAlias + "@ws"
                };

                client.Cypher
                    .Merge("(manager:Owner { MSAlias: {msalias} })")
                    .OnCreate()
                    .Set("manager = {newManager}")
                    .WithParams(new
                    {
                        msalias = newManager.MSAlias,
                        newManager
                    })
                    .ExecuteWithoutResults();

                // Relate employee and manager
                client.Cypher
                    .Match("(owner:Owner)", "(manager:Owner)")
                    .Where((Owner owner) => owner.MSAlias == newEmployee.MSAlias)
                    .AndWhere((Owner manager) => manager.MSAlias == newManager.MSAlias)
                    .CreateUnique("owner-[:REPORTSTO]->manager")
                    .ExecuteWithoutResults();
            }
        }

        private static void UpdateAssetTag(string file)
        {
            Dictionary<string, string> devicesWithSN = new Dictionary<string, string>();
            Dictionary<string, string> devicesWithOldAsset = new Dictionary<string, string>();

            StreamReader sr = new StreamReader(file, true);
            while (!sr.EndOfStream)
            {
                string[] line = sr.ReadLine().Split('\t');
                string oldAsset = line[0];
                string sn = line[1];
                string asset = line[2];

                if (string.IsNullOrWhiteSpace(oldAsset))
                {
                    if (devicesWithSN.ContainsKey(sn))
                    {
                        devicesWithSN[sn] = asset;
                    }
                    else
                    {
                        devicesWithSN.Add(sn, asset);
                    }
                }
                else
                {
                    if (devicesWithOldAsset.ContainsKey(oldAsset))
                    {
                        devicesWithOldAsset[oldAsset] = asset;
                    }
                    else
                    {
                        devicesWithOldAsset.Add(oldAsset, asset);
                    }
                }
            }

            var client = new GraphClient(new Uri("http://neo4j:111111@localhost:7474/db/data"));
            client.Connect();

            foreach (string deviceToUpdate in devicesWithSN.Keys)
            {
                // Prepare device
                string sn = deviceToUpdate;
                string assetTag = devicesWithSN[sn];

                // Set asset tag
                client.Cypher
                    .Match("(device:Device)")
                    .Where((Device device) => device.SN == sn)
                    .Set("device.AssetTag = {assettag}")
                    .WithParam("assettag", assetTag)
                    .ExecuteWithoutResults();
            }

            foreach (string deviceToUpdate in devicesWithOldAsset.Keys)
            {
                // Prepare device
                string asset = deviceToUpdate;
                string assetTag = devicesWithOldAsset[asset];

                // Set asset tag
                client.Cypher
                    .Match("(device:Device)")
                    .Where((Device device) => device.WSAsset == asset)
                    .Set("device.AssetTag = {assettag}")
                    .WithParam("assettag", assetTag)
                    .ExecuteWithoutResults();
            }
        }
    }
}
