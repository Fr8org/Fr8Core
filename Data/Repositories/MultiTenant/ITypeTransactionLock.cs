using System;
using System.Collections.Generic;

namespace Data.Repositories.MultiTenant
{
    public interface ITypeTransactionLock : IDisposable
    {
        bool IsTypePersisted(Guid typeId);
        void Commit(IEnumerable<Guid> typeIds);
    }
}