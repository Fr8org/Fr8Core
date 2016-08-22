using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Data.Repositories.MultiTenant;
using Data.Repositories.MultiTenant.Queryable;
using Fr8.Infrastructure.Data.Manifests;

namespace Data.Interfaces
{
    public interface IMultiTenantObjectRepository
    {
        MtTypeReference FindTypeReference(Type clrType);
        MtTypeReference FindTypeReference(Guid typeId);
        MtTypeReference FindTypeReference(string alias);

        MtTypeReference[] ListTypeReferences();
        MtTypePropertyReference[] ListTypePropertyReferences(Guid typeId);

        void Add(Manifest manifest, string curFr8AccountId);
        void AddOrUpdate(string curFr8AccountId, Manifest curManifest);

        void AddOrUpdate<T>(string curFr8AccountId, T manifest, Expression<Func<T, bool>> @where)
            where T : Manifest;

        void Update(string fr8AccountId, Manifest manifest);

        void Update<T>(string fr8AccountId, T manifest, Expression<Func<T, bool>> @where) 
            where T : Manifest;

        void Delete<T>(string fr8AccountId, Expression<Func<T, bool>> @where) 
            where T : Manifest;
        
        IMtQueryable<T> AsQueryable<T>(string fr8AccountId)
            where T : Manifest;

        List<T> Query<T>(string fr8AccountId, Expression<Func<T, bool>> @where)
            where T : Manifest;

        int Count<T>(string fr8AccountId, Expression<Func<T, bool>> where)
            where T : Manifest;

        Guid? GetObjectId<T>(string fr8AccountId, Expression<Func<T, bool>> where)
            where T : Manifest;
    }
}