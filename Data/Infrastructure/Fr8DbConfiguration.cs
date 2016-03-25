using System.Data.Entity;
using System.Data.Entity.SqlServer;

namespace Data.Infrastructure
{
    public class Fr8DbConfiguration : DbConfiguration
    {
        public Fr8DbConfiguration()
        {
#if DEBUG
            SetExecutionStrategy("System.Data.SqlClient", () => new SqlAzureExecutionStrategy(10, System.TimeSpan.FromSeconds(60)));
#else
            SetExecutionStrategy("System.Data.SqlClient", () => new SqlAzureExecutionStrategy());
#endif
        }
    }
}
