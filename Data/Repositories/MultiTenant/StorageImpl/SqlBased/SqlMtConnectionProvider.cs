using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Infrastructure;
using Data.Interfaces;

namespace Data.Repositories.MultiTenant.SqlBased
{
    class SqlMtConnectionProvider : IMtConnectionProvider
    {
        private readonly IUnitOfWork _uow;

        public object ConnectionInfo
        {
            get
            {
                var adapter = (IObjectContextAdapter)_uow.Db;
                var builder = new EntityConnectionStringBuilder(adapter.ObjectContext.Connection.ConnectionString);

                System.Console.WriteLine(builder.ProviderConnectionString);

                return builder.ProviderConnectionString;
            }
        }

        public SqlMtConnectionProvider(IUnitOfWork uow)
        {
            _uow = uow;
        }
    }
}
