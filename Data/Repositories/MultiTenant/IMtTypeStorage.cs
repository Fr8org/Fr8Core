using System;

namespace Data.Repositories.MultiTenant
{
    internal interface IMtTypeStorage
    {
        MtTypeDefinition ResolveType(IMtConnectionProvider connectionProvider, Type clrType, IMtTypeStorageProvider typeStorageProvider, bool storeIfNew);
        ITypeTransactionLock AccureTypeTransactionLock();
    }
}