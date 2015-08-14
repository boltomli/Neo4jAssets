using Neo4jClient;

namespace AssetsView.Models
{
    public class Employee
    {
        public long nodeId { get; set; }
        public NodeReference noderef { get; set; }
        public string msAlias { get; set; }
        public string wsAlias { get; set; }
        public string email { get; set; }
    }
}
