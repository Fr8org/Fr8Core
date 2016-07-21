using System.Collections.Generic;
using Fr8.Infrastructure.Data.Manifests;

namespace Data.Repositories.MultiTenant.Queryable
{
    // Provides functionality of querying MT DB
    public interface IMtQueryable<T> : IEnumerable<T>
        where T : Manifest
    {
        IMtQueryable<T> Previous { get; }
        IMtQueryExecutor<T> Executor { get; }
    }
}
