namespace Data.Repositories.MultiTenant.InMemory
{
    public class DummyConnectionProvider : IMtConnectionProvider
    {
        public object ConnectionInfo
        {
            get { return null; }
        }
    }
}
