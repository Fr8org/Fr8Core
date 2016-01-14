using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Data.Interfaces.Manifests;
using Data.Repositories;

namespace Data.Interfaces
{
    public interface IMultiTenantObjectRepository
    {
        void Add(IUnitOfWork uow, Manifest curManifest, string curFr8AccountId);

        void AddOrUpdate<T>(IUnitOfWork uow, string curFr8AccountId, T curManifest, Expression<Func<T, object>> keyProperty)
            where T : Manifest;

        void Update<T>(IUnitOfWork uow, string curFr8AccountId, T curManifest, Expression<Func<T, object>> keyProperty) 
            where T : Manifest;

        void Remove<T>(IUnitOfWork uow, string curFr8AccountId, Expression<Func<T, object>> conditionOnKeyProperty, int manifestId = -1) 
            where T : Manifest;
        
        T Get<T>(IUnitOfWork uow, string curFr8AccountId, Expression<Func<T, object>> conditionOnKeyProperty, int manifestId = -1) 
            where T : Manifest;

        IMtQueryable<T> AsQueryable<T>(IUnitOfWork uow, string curFr8AccountId)
            where T : Manifest;

        List<T> Query<T>(IUnitOfWork uow, string curFr8AccountId, Expression<Func<T, bool>> query)
            where T : Manifest;
    }
}