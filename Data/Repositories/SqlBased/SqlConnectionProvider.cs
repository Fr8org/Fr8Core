using System.Configuration;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Repositories.MultiTenant;

namespace Data.Repositories.SqlBased
{
    class SqlMtConnectionProvider : ISqlConnectionProvider
    {
        private readonly IUnitOfWork _uow;

        public object ConnectionInfo
        {
            get
            {
                // Commented out by yakov.gnusin.
                // ProviderConnectionString does not provide Password setting when extracted from IObjectContextAdapter.
                // var adapter = (IObjectContextAdapter)_uow.Db;
                // var builder = new EntityConnectionStringBuilder(adapter.ObjectContext.Connection.ConnectionString);
                //
                // return builder.ProviderConnectionString;

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

        public SqlMtConnectionProvider(IUnitOfWork uow)
        {
            _uow = uow;
        }
    }
}
