using System.Collections.Generic;
using Data.Interfaces.Manifests;

namespace Data.Repositories
{
    // An entity that is capable of executing queries represented by the given IMtQueryable
    public interface IMtQueryExecutor<T>
        where T : Manifest
    {
        IEnumerator<T> RunQuery(IMtQueryable<T> queryable);
    }
}