using System.Collections.Generic;
using System.Linq;

namespace Data.Utility
{
    //This class is used to provide correct pagination
    public static class PagingExtensions
    {
        //used by LINQ to SQL: Queries are ran on DB RAM
        //This signature is preferable for large tables, like [dbo].[History]
        public static IQueryable<TSource> Page<TSource>(this IQueryable<TSource> source, int page, int pageSize)
        {
            return source.Skip((page - 1) * pageSize).Take(pageSize);
        }

        //used by LINQ: Queries are ran on Server RAM
        public static IEnumerable<TSource> Page<TSource>(this IEnumerable<TSource> source, int page, int pageSize)
        {
            return source.Skip((page - 1) * pageSize).Take(pageSize);
        }

    }
}
