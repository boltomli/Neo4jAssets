using Neo4jClient;
using System;

namespace Neo4jAssets
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
    }

    class Program
    {
        static void Main(string[] args)
        {
            var client = new GraphClient(new Uri("http://neo4j:111111@localhost:7474/db/data"));
            client.Connect();

            // Create a device when it doesn't exist. SN is used as unique key, which is usually safe.
            var newDevice = new Device { SN = "12345678", WSAsset = "N/A" };
            client.Cypher
                .Merge("(device:Device { SN: {sn} })")
                .OnCreate()
                .Set("device = {newDevice}")
                .WithParams(new
                {
                    sn = newDevice.SN,
                    newDevice
                })
                .ExecuteWithoutResults();

            // Create a owner and relate to the device we just created. Alias is used as unique key.
            var newOwner = new Owner { MSAlias = "msalias", WSAlias = "wsalias", Email = "msalias@ms.ms" };
            client.Cypher
                .Match("(device:Device)")
                .Where((Device device) => device.SN == "12345678")
                .Merge("device<-[:OWNS]-(owner:Owner { MSAlias: {msalias} })")
                .OnCreate()
                .Set("owner = {newOwner}")
                .WithParams(new
                {
                    msalias = newOwner.MSAlias,
                    newOwner
                })
                .ExecuteWithoutResults();
        }
    }
}
