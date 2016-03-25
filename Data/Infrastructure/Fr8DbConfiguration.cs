using System.Data.Entity;
using System.Data.Entity.SqlServer;

namespace Data.Infrastructure
{
    public class Fr8DbConfiguration : DbConfiguration
    {
        public Fr8DbConfiguration()
        {
            SetExecutionStrategy("System.Data.SqlClient", () => new SqlAzureExecutionStrategy());
        }
    }
}
