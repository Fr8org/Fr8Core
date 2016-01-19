using System.Collections.Generic;
using Data.Interfaces.Manifests;

namespace Data.Repositories
{
    // Provides functionality of querying MT DB
    public interface IMtQueryable<T> : IEnumerable<T>
        where T : Manifest
    {
        IMtQueryable<T> Previous { get; }
        IMtQueryExecutor<T> Executor { get; }
    }
}
