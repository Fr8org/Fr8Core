using System.Collections.Generic;
using Fr8.Infrastructure.Data.Manifests;

namespace Data.Repositories.MultiTenant.Queryable
{
    // An entity that is capable of executing queries represented by the given IMtQueryable
    public interface IMtQueryExecutor<T>
        where T : Manifest
    {
        IEnumerator<T> RunQuery(IMtQueryable<T> queryable);
        int Count(IMtQueryable<T> queryable);
    }
}