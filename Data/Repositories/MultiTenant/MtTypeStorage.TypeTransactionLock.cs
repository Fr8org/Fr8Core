using System;
using System.Collections.Generic;
using System.Threading;

namespace Data.Repositories.MultiTenant
{
    partial class MtTypeStorage
    {
        class TypeSaveLock : ITypeTransactionLock
        {
            private readonly object _sync;
            private readonly MtTypeStorage _storage;

            public TypeSaveLock(object sync, MtTypeStorage storage)
            {
                _sync = sync;
                _storage = storage;
                Monitor.Enter(sync);
            }

            public bool IsTypePersisted(Guid typeId)
            {
                return _storage.CheckTypePersisted(typeId);
            }

            public void Commit(IEnumerable<Guid> types)
            {
                foreach (var type in types)
                {
                    _storage.ConfirmTypePersistence(type);
                }
            }

            public void Dispose()
            {
                Monitor.Exit(_sync);
            }
        }
    }
}
