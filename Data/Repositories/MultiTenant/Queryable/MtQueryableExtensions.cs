using System;
using System.Linq.Expressions;
using Data.Interfaces.Manifests;

namespace Data.Repositories
{
    public static class MtQueryableExtensions
    {
        public static IMtQueryable<T> Where<T>(this IMtQueryable<T> that, Expression<Func<T, bool>> condition)
            where T : Manifest
        {
            return new MtWhere<T>(that, condition);
        }
    }
}