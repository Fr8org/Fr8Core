using System;

namespace Data.Repositories.MultiTenant
{
    internal interface IMtTypeStorage
    {
        MtTypeDefinition ResolveType(Type clrType, IMtTypeStorageProvider typeStorageProvider, bool storeIfNew);
        ITypeTransactionLock AccureTypeTransactionLock();
    }
}