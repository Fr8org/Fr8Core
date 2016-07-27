using System;
using System.Collections.Generic;
using Hub.Infrastructure;
using Data.Interfaces;
using Data.Repositories.MultiTenant.Queryable;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Hub.Services
{
    static partial class MTSearchHelper
    {
        // This class can be used to query MT DB using criterias from Query Builder control.
        // We need a way to query MT DB using manifest type that is unknown at the run-time. 
        // We want to use reflection as little as possible. So we create generic class for converting Query Builder filters and will create instance if this class using reflection. To access members of this class we will use non-generic interface.
        // Se creating new instance will be the only place reflection is used.
        private class MtQueryProvider<T> : IMtQueryProvider
            where  T : Fr8.Infrastructure.Data.Manifests.Manifest
        {
            public Type Type
            {
                get { return typeof (T); }
            }

            public List<object> Query(IUnitOfWork uow, string accountId, List<FilterConditionDTO> conditions)
            {
                var queryable = uow.MultiTenantObjectRepository.AsQueryable<T>(accountId);
                var result = new List<object>();

                 result.AddRange(CriteriaToMtQuery(conditions, queryable));

                return result;
            }

            public void Delete(IUnitOfWork uow, string accountId, List<FilterConditionDTO> conditions)
            {
                var predicateBuilder = new FilterConditionPredicateBuilder<T>(conditions);
                uow.MultiTenantObjectRepository.Delete<T>(accountId, predicateBuilder.ToPredicate());
            }

            private static IMtQueryable<T> CriteriaToMtQuery(
                List<FilterConditionDTO> conditions, IMtQueryable<T> queryable)
            {
                var predicateBuilder = new FilterConditionPredicateBuilder<T>(conditions);
                queryable = queryable.Where(predicateBuilder.ToPredicate());

                return queryable;
            }
        }
    }
}