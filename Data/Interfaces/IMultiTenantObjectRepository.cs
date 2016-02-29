using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Data.Interfaces.Manifests;
using Data.Repositories.MultiTenant;
using Data.Repositories.MultiTenant.Queryable;

namespace Data.Interfaces
{
    public interface IMultiTenantObjectRepository
    {
        MtTypeReference FindTypeReference(Type clrType);
        MtTypeReference FindTypeReference(Guid typeId);
        MtTypeReference[] ListTypeReferences();
        MtTypePropertyReference[] ListTypePropertyReferences(Guid typeId);

        void Add(Manifest curManifest, string curFr8AccountId);
        void AddOrUpdate(string curFr8AccountId, Manifest curManifest);

        void AddOrUpdate<T>(string curFr8AccountId, T curManifest, Expression<Func<T, bool>> @where = null)
            where T : Manifest;

        void Update<T>(string curFr8AccountId, T curManifest, Expression<Func<T, bool>> @where = null) 
            where T : Manifest;

        void Delete<T>(string curFr8AccountId, Expression<Func<T, bool>> @where) 
            where T : Manifest;
        
        IMtQueryable<T> AsQueryable<T>(string curFr8AccountId)
            where T : Manifest;

        List<T> Query<T>(string curFr8AccountId, Expression<Func<T, bool>> @where)
            where T : Manifest;
    }
}