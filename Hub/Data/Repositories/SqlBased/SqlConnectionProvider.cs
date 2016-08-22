using System.Configuration;
using Data.Infrastructure;

namespace Data.Repositories.SqlBased
{
    public class SqlConnectionProvider : ISqlConnectionProvider
    {
        public object ConnectionInfo
        {
            get
            {
                // At runtime ConfigurationManager.ConnectionStrings provides both Azure and Local support.
                // Settings from Azure portal override settings from web.config.
                string connectionDetails = DockyardDbContext.GetEFConnectionDetails();
                if (connectionDetails.StartsWith("name="))
                {
                    // Handle connection string name
                    string cnName = connectionDetails.Substring(5);
                    return ConfigurationManager.ConnectionStrings[cnName].ConnectionString;
                }
                else
                {
                    // Actual connection string
                    return connectionDetails;
                }
            }
        }
    }
}
