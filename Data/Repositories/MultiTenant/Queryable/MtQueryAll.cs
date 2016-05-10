using Fr8Data.Manifests;

namespace Data.Repositories.MultiTenant.Queryable
{
    public class MtQueryAll<T> : MtQueryable<T>
        where T : Manifest
    {
        public MtQueryAll(IMtQueryExecutor<T> executor)
            : base(null, executor)
        {
        }
    }
}