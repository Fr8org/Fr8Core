using Data.Repositories.SqlBased;

namespace Data.Repositories.MultiTenant.InMemory
{
    public class DummyConnectionProvider : ISqlConnectionProvider
    {
        public object ConnectionInfo
        {
            get { return null; }
        }
    }
}
