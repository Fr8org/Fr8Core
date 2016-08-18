using System;
using System.Linq.Expressions;
using Fr8.Infrastructure.Data.Manifests;

namespace Data.Repositories.MultiTenant.Queryable
{
    public static class MtQueryableExtensions
    {
        public static IMtQueryable<T> Where<T>(this IMtQueryable<T> that, Expression<Func<T, bool>> condition)
            where T : Manifest
        {
            return new MtWhere<T>(that, condition);
        }

        public static int MtCount<T>(this IMtQueryable<T> that)
            where T : Manifest
        {
            return that.Executor.Count(that);
        }

        public static int MtCount<T>(this IMtQueryable<T> that, Expression<Func<T, bool>> condition)
          where T : Manifest
        {
            var where = new MtWhere<T>(that, condition);
            return where.Executor.Count(where);
        }
    }
}